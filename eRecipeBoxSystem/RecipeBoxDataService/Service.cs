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
using DataStoreUtils;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using RecipeBoxSolutionModel;
using static DataStoreUtils.DbConnection;
namespace RecipeBoxDataService
{
    public class Service : DataStoreService
    {
        public static IDataStore DataStore;
        static Service()
        {
            if (DataStore == null)
                CreateDataStore();
        }
        static public void CreateDataStore()
        {
            DataStore = XpoService.ConnectDefaultDataStore(ModelInfo.PersistentTypes, "Recipes");
            using (UnitOfWork uow = new UnitOfWork())
            {
                //#Determine which from the conn string  
                string XpoConnStr = ConnectionStrings.GetConnectionString("Recipes");
                DbProvider provider = XpoService.GetDbProvider(XpoConnStr);
                string cmd = RecipeBoxSolutionModel.SQL.RecipeCardSQL.GenerateConstraintAndIndexesSQL(provider.ToString());
                uow.ExecuteNonQuery(cmd);
            }
        }
        public Service() : base(DataStore)
        {

            //https://supportcenter.devexpress.com/ticket/details/e5072/how-to-implement-a-distributed-object-layer-service-working-via-wcf
            //#RQT Network config  
            //1. need to run this cmd in order to listen on this port
            //netsh http add urlacl url=http://+:5757/service user=YOURUSER
            //how to delete https://docs.microsoft.com/en-us/windows/win32/http/delete-urlacl
            //netsh http delete urlacl url=http://+:5757/service  
            //1a. netsh http add urlacl url=https://+:5757/service user=fednode4\rick_2
            //1a. netsh http add urlacl url=https://+:5757/service user=rickpc3\rmcum
            //for ss https wsHttpBinding with security, need to assign a trusted cert to the port 
            //to do so, run createcert.bat.  if you dont, the client gets weird error - client and server have incompat config
            // this is needed on the client when working with test/selfsigned certs
            //https://stackoverflow.com/questions/1742938/how-to-solve-could-not-establish-trust-relationship-for-the-ssl-tls-secure-chan
            //this calls makecert that is in this windows sdk - SDK do Windows 10 (10.0.10240)
            //https://developer.microsoft.com/en-us/windows/downloads/sdk-archive/
            //1a. if using https, need to add a cert to the client pc
            //2. test this service locally.
            //3. deploy to app svr.  be sure to open firwall for port 5757.  inbound rule
            //4. open to outside world.  config router to forward the port to the app server
            //https://opensource.com/article/20/9/firewall
            //5. external url is router's external ip http://icanhazip.com/  216.164.51.161
            //site with instructions https://portforward.com/netgear/orbi-rbr50/
            //portchecker.co
            //windows tcpview utility
            //6. test by trying ext url in browser http://216.164.61.143:5757/service
            //7. godaddy.com and set the ip address for cookslikeme.com.  ping cookslikeme.com


            //to install in prod
            //plug app server into external router/gateway
            //set fixed IP address to the app server (so it is always the same)
            //in external router, forward 5757 port to the app server
            //if in dmz.  need to forward the sql server port 1433 to the sql server box in the internnal router. sql server box needs a fixed ip address

            //how to install a windows service using cmd line
            // to install me
            // admin console 
            // cd C:\Windows\Microsoft.NET\Framework64\v4.0.30319
            // installutil.exe -i C:\ROM\r3projects\Redshift\killerapp\Recipes\WindowsRecipeService\bin\Debug\WindowsRecipeService.exe
            // installutil.exe -u C:\ROM\r3projects\Redshift\killerapp\Recipes\WindowsRecipeService\bin\Debug\WindowsRecipeService.exe

            //triage router
            //https://customer.cradlepoint.com/s/article/How-To-Troubleshoot-Port-Forwarding-Issues
            //https://www.whatismyip.com/my-ip-information/
            //https://www.yougetsignal.com/tools/open-ports/

            //potential prob. verify router wan is same as
            //https://www.reddit.com/r/Blackops4/comments/9o7o1o/psa_you_cant_forward_ports_if_youre_behind_a/
            ////https://www.netgear.com/support/download/?model=genie
            ///

            /*https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/programming-wcf-security
             * If you decide to use transport security for HTTP (in other words, HTTPS), you must also configure the host with an SSL certificate and enable SSL on a port. For more information, see HTTP Transport Security.
             * https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/http-transport-security
             * If you are creating a self-hosted WCF application, you can bind an SSL certificate to the address using the HttpCfg.exe tool.
             * https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-configure-a-port-with-an-ssl-certificate
             * */

        }
    }
}
