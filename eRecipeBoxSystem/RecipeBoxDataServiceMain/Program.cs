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
using System;
using System.IO;
using System.ServiceModel;
namespace RecipeBoxDataServiceMain
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //instructions are in RecipeBoxDataService.Service
                ServiceHost h = new ServiceHost(typeof(RecipeBoxDataService.Service));
                h.Open();
                Console.ReadLine();

                StreamWriter log = new StreamWriter("errorlog.txt");
                log.WriteLine("readline done");
                log.Close();

            }
            catch (Exception ex)
            {
                StreamWriter log = new StreamWriter("errorlog.txt");
                log.WriteLine(ex.Message);
                log.Close();
                Console.WriteLine("exception: " + ex.Message);
                Console.ReadLine();

                throw;
            }
        }
    }
}
/*
 * 
 'To use with SSL:
'- Check if certificate already registered for port 5775: 
'  netsh http show sslcert ipport=0.0.0.0:5757
'- If certificate already exists, delete it with: 
'  netsh http delete sslcert ipport=0.0.0.0:5757
'- Create certificate for localhost:
'  "C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\x64\makecert" -ss My -sr LocalMachine -n CN=localhost -r
'  This will create certificate in:
'  Certificates Snap-in/Certificates (Local Computer)/Personal/Certificates/localhost
'- Certificates Snap-in can be viewed with:
'  Start/Run/mmc/File/Add/Remove Snap-in.../Certificates/Add/Computer account/Next/Local computer/Finish
'- Make certificate trusted. In Certificates Snap-in copy-paste:
'  from Personal/Certificates/localhost to Trusted People/Certificates.
'- Copy thumbprint of certificate to clipboard:
'  Double click on Personal/Certificates/localhost, then select tab page Details, clieck on Field Thumbprint
'  and select text in textbox and Ctrl-C.
'- Add certificate to port:
'  netsh http add sslcert ipport=0.0.0.0:5757 certhash=<certificate thumb print> appid={12345678-1234-1234-1234-123456789012}
'  appid seems not to play a role.

<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.serviceModel>
    <behaviors>
      <serviceBehaviors>
        <behavior name="TestServiceBehavior" >
          <serviceMetadata httpsGetEnabled="true" />
        </behavior>
      </serviceBehaviors>
    </behaviors>
    <services>
      <service name="WCFService.TestService" behaviorConfiguration="TestServiceBehavior">
        <endpoint address="" binding="wsHttpBinding" bindingConfiguration="transactionalBinding" contract="WCFService.ITestService" />
        <endpoint address="mex" binding="mexHttpsBinding" contract="IMetadataExchange" />
        <host>
          <baseAddresses>
            <add baseAddress="https://computer1.domain1.com:5757" />
          </baseAddresses>
        </host>
      </service>
    </services>
    <bindings>
      <wsHttpBinding>
        <binding name="transactionalBinding" transactionFlow="true">
          <security mode="Transport">
            <transport clientCredentialType="None"/>
          </security>
        </binding>
      </wsHttpBinding>
    </bindings>
  </system.serviceModel>  
</configuration>

*/
