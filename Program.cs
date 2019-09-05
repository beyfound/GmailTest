using System;
using System.Net;
using System.Net.Mail;

namespace GmailSender
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(args[0], args[1]);
                    mail.To.Add(args[3]);
                    mail.Subject = args[4];
                    mail.Body = args[5];
                    mail.IsBodyHtml = true;
                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential(args[0], args[2]);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);

                    }
                }
                Console.WriteLine("Send mail successfully.");
                Environment.Exit(0);
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Error : {exc.Message}");
                Environment.Exit(-1);
            }
        }
    }
}
