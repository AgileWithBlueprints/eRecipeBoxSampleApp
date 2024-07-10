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
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Foundation;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace DataStoreUtils
{
    partial class XpoService
    {
        #region connection helpers
        private static IDataStore GetWSHttpDataStore(string url, string user, string pw)
        {

            //generate service reference
            //https://docs.devexpress.com/XPO/10018/connect-to-a-data-store/transfer-data-via-wcf-services
            //https://supportcenter.devexpress.com/ticket/details/e4930/how-to-connect-to-a-remote-data-service-instead-of-using-a-direct-database-connection

            //https://supportcenter.devexpress.com/ticket/details/e5137/how-to-connect-to-remote-data-store-and-configure-wcf-end-point-programmatically

            EndpointAddress address = new EndpointAddress(url);

            //Use this for basic Http
            //BasicHttpBinding binding = new BasicHttpBinding();
            //binding.MaxBufferSize = Int32.MaxValue;

            //HTTPS over WS* web service specs
            WSHttpBinding binding = new WSHttpBinding
            {
                MaxReceivedMessageSize = Int32.MaxValue
            };
            binding.ReaderQuotas.MaxNameTableCharCount = Int32.MaxValue;
            binding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            binding.ReaderQuotas.MaxDepth = Int32.MaxValue;
            binding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
            binding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;

            //binding.Security.Mode = BasicHttpSecurityMode.TransportWithMessageCredential; //BasicHttpBinding
            binding.Security.Mode = SecurityMode.TransportWithMessageCredential; //WSHttpBinding

            //binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;  //BasicHttpBinding
            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName; //WSHttpBinding

            try
            {

                //#TODO IMPORTANT WARNING!! this is only for dev only... it accepts any cert including unsigned
                //It allows us to accept our self-signed cert host running our DataService on an app server.
                //For Prod, get a signed cert and remove this code.
                //https://stackoverflow.com/questions/1742938/how-to-solve-could-not-establish-trust-relationship-for-the-ssl-tls-secure-chan
                System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                    (se, cert, chain, sslerror) =>
                    {
                        return true;
                    };

                IDataStore store = new DataStoreClient(binding, address);
                var factory = ((DataStoreClient)store).ChannelFactory; // fetch the Microsoft WCF channel factory  
                //set username and pw
                factory.Credentials.UserName.UserName = user;
                factory.Credentials.UserName.Password = pw;

                store.AutoCreateOption.ToString();//call the server
                return store;
            }
            catch (Exception e)
            {

                throw new DevExpress.Xpo.DB.Exceptions.UnableToOpenDatabaseException(url, e);
            }
        }
        private static IDataStore GetHTTPDataStore(string url)
        {
            EndpointAddress address = new EndpointAddress(url);
            BasicHttpBinding binding = new BasicHttpBinding
            {
                MaxBufferSize = Int32.MaxValue,
                MaxReceivedMessageSize = Int32.MaxValue
            };
            binding.ReaderQuotas.MaxNameTableCharCount = Int32.MaxValue;
            binding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
            binding.ReaderQuotas.MaxDepth = Int32.MaxValue;
            binding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
            binding.ReaderQuotas.MaxStringContentLength = Int32.MaxValue;
            try
            {
                IDataStore store = new DataStoreClient(binding, address);
                store.AutoCreateOption.ToString();
                return store;
            }
            catch (Exception e)
            {
                throw new DevExpress.Xpo.DB.Exceptions.UnableToOpenDatabaseException(url, e);
            }
        }
        private static void UpdateModelVersions()
        {
            using (UnitOfWork uow = new UnitOfWork())
            {
                foreach (ModelVersion ver in ModelVersion.RegisteredDataStoreComponentVersions)
                {
                    ModelVersion.PersistDataStoreComponentVersion(ver, uow);
                }
            }
        }
        #endregion

        #region static helper methods        
        static private string DisplayString(OperandValue[] operandValues)
        {
            List<string> vals = new List<string>();
            foreach (OperandValue ov in operandValues)
            {
                //Note: We don't log Int criteriaParametersList values because they contain Oids that will differ across Test executions
                if (ov.Value is Int32)
                    continue;
                vals.Add(ov.Value.ToString());
            }
            return String.Join(",", vals);
        }

        static private void LogHeadBOAction(string action, HeadBusinessObject headBusObject)
        {
            Log.DataStore.Info($"{action}|{headBusObject.SerializeMe()}");
        }
        #endregion
    }
}
