/*
* MIT License
* 
* Copyright (C) 2024 SoftArc, LLC
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/
using Npgsql;
using System.Collections.Generic;
using System.Text;

namespace DataStoreUtils.DbConnectionAdapters
{
    internal class NpgSqlConnection : DbConnection
    {
        internal NpgSqlConnection(string connString, DbProvider dbProvider) : base(connString, dbProvider)
        {
            dbConnection = new NpgsqlConnection(connString);
        }
        public override string SQLTableName(string tableName)
        {
            return '"' + tableName + '"';
        }
        public override string SQLColumnName(string colName)
        {
            return '"' + colName + '"';
        }

        public override void LoadBinaryFileToTable(string filePath, int batchSize = 100000)
        {
            BinaryTableFileReader infile = new BinaryTableFileReader(filePath, null);
            //no data, nothing to load
            if (infile.ColumnInfos.Count == 0)
                return;
            //https://csharp.hotexamples.com/examples/Npgsql/NpgsqlConnection/BeginBinaryImport/php-npgsqlconnection-beginbinaryimport-method-examples.html
            StringBuilder cpyCMD = new StringBuilder();
            cpyCMD.Append("COPY");
            cpyCMD.Append($@" ""{infile.TableName}""(");
            List<string> colsList = new List<string>();
            foreach (DbColumnInfo ci in infile.ColumnInfos)
            {
                colsList.Add($@"""{ci.Name}""");
            }
            cpyCMD.Append(string.Join(",", colsList));
            cpyCMD.Append(") from STDIN (format binary)");

            //example @"COPY ""CookedDish""(""OID"", ""RecipeCard"", ""CookedDate"", ""OptimisticLockField"", ""GCRecord"") from STDIN (format binary)";"
            var x = cpyCMD.ToString();
            using (NpgsqlBinaryImporter writer = ((NpgsqlConnection)this.dbConnection).BeginBinaryImport(x))
            {
                while (infile.Read())
                {
                    writer.StartRow();
                    for (int i = 0; i < infile.FieldCount; i++)
                    {
                        writer.Write(infile.GetValue(i));
                    }
                }
                writer.Complete();
            }
        }

        public override void EnableAllFKsAndRebuildIndexes()
        {
            string enableCMD = @"

--resume triggers and constraint checks
SET session_replication_role = 'origin';

DO $$ DECLARE
    r record;
BEGIN
    FOR r IN (
        SELECT conrelid::regclass AS table_name, conname
        FROM pg_constraint
        WHERE confrelid IS NOT NULL AND contype = 'f' AND
        connamespace = (SELECT oid FROM pg_namespace WHERE nspname = 'public')
    ) LOOP
        EXECUTE 'ALTER TABLE ' || r.table_name || ' ENABLE TRIGGER ALL; ';
    END LOOP;
 
END $$;


--#FRAGILE  this assumes we are using public schema!!  need to set all PK sequences to max(largest PK)
--#WORKAROUND seems like the generated backup ddl should do this for us
DO
$$
DECLARE
    rec RECORD;
BEGIN
    FOR rec IN
with mytables as (
SELECT c.relname, CONCAT('public.""',c.relname,'""')   AS fulltablename        
FROM pg_class c
JOIN pg_namespace n ON c.relnamespace = n.oid
WHERE n.nspname = 'public' and  c.relkind = 'r')
        SELECT
            c.relname AS table_name,
            a.attname AS column_name,
            pg_get_serial_sequence(mt.fulltablename, a.attname) AS seq_name
        FROM
            pg_class c
            JOIN pg_namespace n ON n.oid = c.relnamespace
            JOIN pg_attribute a ON a.attrelid = c.oid
            JOIN pg_constraint ct ON ct.conrelid = c.oid AND a.attnum = ANY(ct.conkey)
            JOIN pg_attrdef ad ON ad.adrelid = c.oid AND ad.adnum = a.attnum
	    JOIN mytables mt on mt.relname=c.relname
        WHERE
            c.relkind = 'r' -- only real tables
            AND n.nspname = 'public' -- assuming public schema; adjust as necessary
            AND ct.contype = 'p' -- primary key constraint
            AND pg_get_serial_sequence(mt.fulltablename, a.attname) IS NOT NULL
    LOOP
        EXECUTE format('SELECT setval(''%s'', COALESCE(MAX(%I), 0) + 1) FROM %I', rec.seq_name, rec.column_name, rec.table_name) INTO rec;
    END LOOP;
END
$$;
";
            ExecuteNonQuery(enableCMD, false, null);
        }

        public override string DropAllTablesDDL()
        {
            //#FRAGILE assumes public schema
            return
@"
DO $$ DECLARE
    r RECORD;
BEGIN
    -- Disable triggers to avoid unnecessary checks during dropping tables
    EXECUTE 'ALTER TABLE ALL IN SCHEMA public DISABLE TRIGGER ALL';

    -- Drop all tables
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = current_schema()) LOOP
        EXECUTE 'DROP TABLE IF EXISTS ' || quote_ident(r.tablename) || ' CASCADE';
    END LOOP;

    -- Enable triggers back
    EXECUTE 'ALTER TABLE ALL IN SCHEMA public ENABLE TRIGGER ALL';
END $$;
";

        }

        public override void DisableAllFKandIndexes()
        {
            //SELECT table_name FROM information_schema.tables WHERE table_schema='public'
            string disableCMD = @"

--bypass triggers and constraint checks
SET session_replication_role = 'replica';

DO $$ 
DECLARE
    r record;
BEGIN
    FOR r IN (SELECT table_name FROM information_schema.tables WHERE table_schema='public') LOOP			  
        EXECUTE 'ALTER TABLE ' || quote_ident(r.table_name) || ' DISABLE TRIGGER ALL;';
    END LOOP;
 
END $$;

";
            ExecuteNonQuery(disableCMD, false, null);

        }
    }
}
