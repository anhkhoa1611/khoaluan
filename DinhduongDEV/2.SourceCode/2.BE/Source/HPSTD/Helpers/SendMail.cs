using HPSTD.Models;
using HPSTD.Core.Entities;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Dapper;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace HPSTD.Helpers
{
    public class SendMail
    {
        static bool invalid = false;
        public static bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names. 
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format. 
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
        public static string convertPhone(string phone)
        {
            string result = "84" + phone.Substring(1, phone.Length - 1);
            return result;
        }
        public void Send(string subject, string content, string listTos, string type, string attach, string DisplayName = "Hệ thống")
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                var mailconfig = dbConn.FirstOrDefault<Emails>(s => s.trang_thai == "true");
                MailMessage mail = new MailMessage();
                if (!string.IsNullOrEmpty(listTos))
                {
                    foreach (var item in listTos.Split(';'))
                    {
                        if (item != "") { mail.To.Add(item); }
                    }
                }
                if (!string.IsNullOrEmpty(type))
                {
                    var listTosDefault = dbConn.Scalar<string>("SELECT Value FROM Parameters WHERE Type = {0} and Status=1", type);
                    if (!string.IsNullOrEmpty(listTosDefault))
                    {
                        foreach (var item in listTosDefault.Split(';'))
                        {
                            if (item != "") { mail.To.Add(item); }
                        }
                    }
                }
                mail.From = new MailAddress(mailconfig.email, DisplayName);
                mail.Body = HttpUtility.HtmlDecode(content);
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Port = mailconfig.port;
                smtp.Host = mailconfig.mail_server;
                smtp.EnableSsl = mailconfig.enable_ssl;
                smtp.UseDefaultCredentials = false;
                smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtp.Credentials = new System.Net.NetworkCredential(mailconfig.email, mailconfig.password);
                if (!string.IsNullOrEmpty(attach))
                {
                    Attachment attachment;
                    attachment = new Attachment(System.Web.Hosting.HostingEnvironment.MapPath("~/attach-file/" + attach));
                    mail.Attachments.Add(attachment);
                }
                mail.Subject = subject;
                ServicePointManager.ServerCertificateValidationCallback =
                    delegate (object s, X509Certificate certificate,
                             X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    { return true; };
                smtp.Send(mail);
            }
        }
        public static async Task<string> SendEmail(string mailTos,string MailType, string UserName, string Password, string Url, string fullname)
        {
            using (var dbConn = Helpers.OrmliteConnection.openConn())
            {
                try
                {
                    MailMessage mail = new MailMessage();
                    string html = "";
                    string mailTo = "", mailCc = "", mailBcc = "";
                    if (MailType == "CreateUser")
                    {
                        var emailConten = dbConn.SingleOrDefault<EmailContent>("mau_email= 'TAO_MOI_NGUOI_DUNG'");
                        html += "<p><strong>Xin ch&agrave;o " + fullname + ",</strong></p>";
                        html += "<p>T&agrave;i khoản đăng nhập v&agrave;o hệ thống  HD_Bank của bạn l&agrave;:<br />";
                        html += "T&agrave;i khoản: " + UserName + "<br />";
                        html += "Mật khẩu: " + Password + "<br />";
                        html += "Truy cập " + Url + " để đăng nhập v&agrave;o hệ thống.</p>";
                        html += emailConten != null ? emailConten.noi_dung : "";
                        mail.Subject = emailConten != null ? emailConten.tieu_de : ""; ;
                        mail.SubjectEncoding = System.Text.Encoding.UTF8;

                        mailTo = emailConten != null ? emailConten.mailTo : "";
                        mailCc = emailConten != null ? emailConten.mailCc : "";
                        mailBcc = emailConten != null ? emailConten.mailBcc : "";

                    }
                    else if (MailType == "ResetPassword")
                    {
                        var emailConten = dbConn.SingleOrDefault<EmailContent>("mau_email= 'CAP_LAI_MAT_KHAU'");
                        html += "<p><strong>Xin ch&agrave;o " + fullname + ",</strong></p>";
                        html += "<p>Mật khẩu của bạn vừa được cập nhật. Th&ocirc;ng tin đăng nhập của bạn v&agrave;o hệ thống HD_Bank l&agrave;:<br />";
                        html += "T&agrave;i khoản: " + UserName + "<br />";
                        html += "Mật khẩu: " + Password + "<br />";
                        html += "Truy cập " + Url + " để đăng nhập v&agrave;o hệ thống.</p>";
                        html += emailConten != null ? emailConten.noi_dung : "";
                        mail.Subject = emailConten != null ? emailConten.tieu_de : ""; ;
                        mail.SubjectEncoding = System.Text.Encoding.UTF8;

                        mailTo = emailConten != null ? emailConten.mailTo : "";
                        mailCc = emailConten != null ?  emailConten.mailCc : "";
                        mailBcc = emailConten != null ? emailConten.mailBcc : "";

                    }
                    mailTos = mailTos + ";" + mailTo;
                    var listmails = mailTos.Split(';').ToList();
                    if (listmails.Count() > 0)
                    {
                        foreach (var item in listmails)
                        {
                            if (IsValidEmail(item))
                            {
                                mail.To.Add(item);
                            }
                        }
                    }
                    var mailconfig = dbConn.FirstOrDefault<Emails>(s => s.trang_thai == "true");

                    mail.From = new MailAddress(mailconfig.email);
                    if (!string.IsNullOrEmpty(mailCc))
                    {
                        mail.CC.Add(mailCc);
                    }
                    if (!string.IsNullOrEmpty(mailBcc))
                    {
                        mail.Bcc.Add(mailBcc);
                    }

                    mail.Body = HttpUtility.HtmlDecode(html);
                    mail.IsBodyHtml = true;
                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                    smtp.Port = mailconfig.port;
                    smtp.Host = mailconfig.mail_server;
                    smtp.EnableSsl = mailconfig.enable_ssl;
                   // smtp.UseDefaultCredentials = false;
                    smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    smtp.Credentials = new System.Net.NetworkCredential(mailconfig.email, mailconfig.password);
                    smtp.Send(mail);
                    return "";
                }
                catch (Exception ex)
                {
                    return "";
                }

            }
        }
    }
}