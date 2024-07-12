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
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeKit;
using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Foundation
{
    public class EmailUtils
    {
        public static void SendEmailMessage(string from, string to, string subject, string body)
        {
            try
            {
                OAuthGmail.SendEmail(from, to, subject, body);
            }
            catch (Exception e)
            {

                if (e.Message.ToLower().Contains("token") && e.Message.ToLower().Contains("revoked"))
                {
                    //token expired, try again.. Google prompts for authentication
                    OAuthGmail.SendEmail(from, to, subject, body);
                }
                else
                    throw e;
            }
        }
    }
    internal class OAuthGmail
    {
        //use this to create mime message
        //https://stackoverflow.com/questions/24728793/creating-a-message-for-gmail-api-in-c-sharp
        static string Encode(MimeMessage mimeMessage)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                mimeMessage.WriteTo(ms);
                return Convert.ToBase64String(ms.GetBuffer())
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }
        }
        /* Global instance of the scopes required by this quickstart.
         If modifying these scopes, delete your previously saved token.json/ folder. */
        static string[] Scopes = { GmailService.Scope.GmailSend };
        static string ApplicationName = "Recipes";
        public static void SendEmail(string from, string to, string subject, string body)
        {
            UsersResource.MessagesResource.SendRequest request = null;
            Message encodedGmailMessage = null;
            GmailService service = null;

            //#TODO If gmail auth goes wrong (eg, user not authorized) die more gracefully.
            try
            {
                string AssemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string gCredentialsFilePath = AssemblyFolder + "\\gCredentials.json";
                UserCredential credential;
                //From: https://developers.google.com/gmail/api/quickstart/dotnet   obtain a service using oauth 2
                // Load client secrets.
                FileInfo fileInfo = new FileInfo(gCredentialsFilePath);
                if (!fileInfo.Exists || fileInfo.Length == 0)
                    throw new MissingCredentialsExeption();
                using (var stream = new FileStream(gCredentialsFilePath, FileMode.Open, FileAccess.Read))
                {
                    /* The file token.json stores the user's access and refresh tokens, and is created
                     automatically when the authorization flow completes for the first time. */
                    string credPath = "token.json";
                    credential = Task.Run(() => GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true))).Result;
                }

                // Create Gmail API service.
                service = new GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });

                //send email using java or python
                //https://developers.google.com/gmail/api/guides/sending

                //.net MailMessage
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(from, "recipes", System.Text.Encoding.UTF8);
                mailMessage.To.Add(to);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                //intermediate MimeMessage 
                MimeMessage mimeMessage = MimeKit.MimeMessage.CreateFromMailMessage(mailMessage);

                //Google email Message
                encodedGmailMessage = new Message
                {
                    Raw = Encode(mimeMessage)
                };

                request = service.Users.Messages.Send(encodedGmailMessage, "me");
                request.Execute();
            }
            catch (Exception ex)
            {
                Log.App.Info($"Error while sending email message: {ex.Message}");
                throw;
            }
        }
    }
    public class MissingCredentialsExeption : Exception
    {
        // Default constructor
        public MissingCredentialsExeption()
        {
        }

        // Constructor that accepts a custom message
        public MissingCredentialsExeption(string message)
            : base(message)
        {
        }

        // Constructor that accepts a custom message and an inner exception
        public MissingCredentialsExeption(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
