using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;


using MongoDB.Bson;
using Newtonsoft.Json;
using MongoDB.Driver;
using System.Linq;
using System.Threading;
using MongoDB.Bson.Serialization;
using cms_api.Models;
using System.Net.Mail;
using System.Net;

namespace cms_api.Extension
{
    public class SendMailService
    {
        //ใช้สำหรับ....

        public SendMailService(string description, string subject, string emailUser)
        {
            _ = this.SendMail(description, subject, emailUser);
        }

        private Task SendMail(string description, string subject, string emailUser)
        {
            try
            {
                var email = new List<string>();
                //email.Add("worawan_p@ksp.or.th");
                email.Add("porntavee29@gmail.com");

                if (!string.IsNullOrEmpty(emailUser))
                    email.Add(emailUser);

                string emailHost = "ex587mail@gmail.com";
                string password = "EX74108520";
                bool enableSsl = true;
                int port = 587;

                SmtpClient client = new SmtpClient("smtp.gmail.com");
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(emailHost, password);
                client.EnableSsl = enableSsl;
                client.Port = port;

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(emailHost, "สัตวแพทยสภา");

                email.ForEach(c =>
                {
                    mailMessage.To.Add(c);
                });

                mailMessage.IsBodyHtml = true;
                //mailMessage.Body = "รหัสผ่านใหม่ของคุณคือ  " + value.newPassword;
                mailMessage.Body = description;
                //mailMessage.Subject = "ยืนยันการเปลี่ยนรหัสผ่าน";
                mailMessage.Subject = subject;
                client.Send(mailMessage);


                //var email = new List<string>();
                ////email.Add("worawan_p@ksp.or.th");
                //email.Add("porntavee29@gmail.com");

                //if (!string.IsNullOrEmpty(emailUser))
                //    email.Add(emailUser);
                
                //var body = new { email = email, title = "สัตวแพทยสภา", description = description, subject = $"{subject}" };
                //var jsonBody = JsonConvert.SerializeObject(body);
                //HttpRequestMessage httpRequest = null;
                //HttpClient httpClient = null;
                ////var authorizationKey = string.Format("key={0}", "AAAA3yUfsSY:APA91bEBuY3wyOsrsXGLpuzPuYfT3xuCHl53VkaUqSs3zPwXCD3Ud7VZXje08hm2gleVJdBHTi7sZxMLoKOPKYhcAiagUsOkcECugDO67RYYViasLF_ZJIYkkBemSA81nT-LibFFLgVn");

                //try
                //{
                //    httpRequest = new HttpRequestMessage(HttpMethod.Post, "http://core148.we-builds.com/email-api/Email/Create");
                //    //httpRequest.Headers.TryAddWithoutValidation("Authorization", authorizationKey);
                //    httpRequest.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
                //    httpClient = new HttpClient();
                //    using (await httpClient.SendAsync(httpRequest)) { }
                //}
                //catch
                //{
                //    throw;
                //}
                //finally
                //{
                //    httpRequest.Dispose();
                //    httpClient.Dispose();
                //}
            }
            catch
            {
                //return new Response { status = "E", message = ex.Message };
            }

            return Task.CompletedTask;
        }
    }
}
