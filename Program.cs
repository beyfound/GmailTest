using S22.Imap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GmailSender
{
    class Program
    {
        //-sAdr sender username
        //-pwd
        //-sDisplay
        //-rAdr
        //- sub
        //-body
        // -s
        //- f reply 
        // -r
        public enum Type
        {
            Create,
            Reply,
            Find
        }
        static string FromAddress { get; set; }
        static string FromPassword { get; set; }
        static string ToAddress { get; set; }
        static string FromName { get; set; }
        static string Subject { get; set; }
        static string Body { get; set; }
        static string FindString { get; set; }
        static string ActionType { get; set; }

        static readonly string imapHost = "imap.gmail.com";
        static readonly string smtpHost = "smtp.gmail.com";
        static readonly string senderName = "Joe Customer";
        static void Main(string[] args)
        {
            InitParameter(args);
            try
            {
                switch (ActionType)
                {
                    case "Create":
                        SendMessage();
                        break;
                    case "Reply":
                        Reply();
                        break;
                    case "Find":
                        FindMessageBySubject();
                        break;
                    default:
                        Console.WriteLine($"Cannot find this this action type: {ActionType}.");
                        Environment.Exit(-1);
                        break;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Error : {exc.Message}");
                Environment.Exit(-1);
            }

        }

        private static void InitParameter(string[] args)
        {
            ActionType = args[0];
            FromAddress = args[1];
            FromPassword = args[2];
            FromName = args[3];
            ToAddress = args[4];
            Subject = args[5];
            Body = args[6];
            FindString = args[7];
        }

        private static void SendMessage()
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(FromAddress, FromName);
                mail.To.Add(ToAddress);
                mail.Subject = Subject;
                mail.Body = Body;
                mail.IsBodyHtml = true;
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(FromAddress, FromPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);

                }
            }
            Console.WriteLine("Send mail successfully.");
        }

        public static int FindMessageByDate(DateTime dTime, string emailAddress, string password)
        {
            ImapClient client = ConnectIMAP(emailAddress, password);
            // Find messages that were sent Since a specified DateTime
            IEnumerable<uint> uids = client.Search(SearchCondition.SentSince(dTime));
            return uids.Count();
        }

        private static void FindMessageBySubject()
        {
            ImapClient client = ConnectIMAP(FromAddress, FromPassword);
            // Find messages that were sent from abc@def.com and have the string "Hello World" in their subject line.
            IEnumerable<uint> uids = client.Search(SearchCondition.Unseen().And(SearchCondition.From(FromAddress)).And(SearchCondition.Subject(Subject)));

            if( uids.Count() !=1)
            {
                throw new Exception($"Find {uids.Count()} messages matchs address: {FromAddress}, subject:{Subject}");
            }

            Console.WriteLine("Find message by subject successfully!");
        }
        public static IEnumerable<MailMessage> GetUnseenMessages(string emailAddress, string password)
        {
            IEnumerable<MailMessage> message;
            ImapClient client = ConnectIMAP(emailAddress, password);


            // Get a collection of all unseen messages in the INBOX folder
            client.DefaultMailbox = "INBOX";
            IEnumerable<uint> uids = client.Search(SearchCondition.Unseen());

            if (uids.Count() == 0)
                message = null;
            //return null;
            else
                message = client.GetMessages(uids);

            return message;
        }

        public static ImapClient ConnectIMAP(string address, string password)
        {
            return new ImapClient(imapHost, 993, address, password, AuthMethod.Auto, true);

        }

        private static MailMessage CreateReply(MailMessage source)
        {
            MailMessage reply = new MailMessage(new MailAddress(FromAddress, "Sender"), source.From);

            // Get message id and add 'In-Reply-To' header
            string id = source.Headers["Message-ID"];
            reply.Headers.Add("In-Reply-To", id);

            // Try to get 'References' header from the source and add it to the reply
            string references = source.Headers["References"];

            if (!string.IsNullOrEmpty(references))
                references += ' ';

            reply.Headers.Add("References", references + id);

            // Add subject
            if (!source.Subject.StartsWith("Re:", StringComparison.OrdinalIgnoreCase))
                reply.Subject = "Re: ";

            reply.Subject += source.Subject;

            // Add body
            StringBuilder body = new StringBuilder();
            if (source.IsBodyHtml)
            {
                body.Append("<p>Thank you for your email!</p>");
                body.Append("<p>We are currently out of the office, but we will reply as soon as possible.</p>");
                body.Append("<p>Best regards,<br>");
                body.Append(senderName);
                body.Append("</p>");
                body.Append("<br>");

                body.Append("<div>");
                //if (source.Date().HasValue)
                //    body.AppendFormat("On {0}, ", source.Date().Value.ToString(CultureInfo.InvariantCulture));

                if (!string.IsNullOrEmpty(source.From.DisplayName))
                    body.Append(source.From.DisplayName + ' ');

                body.AppendFormat("<<a href=\"mailto:{0}\">{0}</a>> wrote:<br/>", source.From.Address);

                if (!string.IsNullOrEmpty(source.Body))
                {
                    body.Append("<blockqoute style=\"margin: 0 0 0 5px;border-left:2px blue solid;padding-left:5px\">");
                    body.Append(source.Body);
                    body.Append("</blockquote>");
                }

                body.Append("</div>");
            }
            else
            {
                body.AppendLine("Thank you for your email!");
                body.AppendLine();
                body.AppendLine("We are currently out of the office, but we will reply as soon as possible.");
                body.AppendLine();
                body.AppendLine("Best regards,");
                body.AppendLine(senderName);
                body.AppendLine();

                //if (source.Date().HasValue)
                //    body.AppendFormat("On {0}, ", source.Date().Value.ToString(CultureInfo.InvariantCulture));

                body.Append(source.From);
                body.AppendLine(" wrote:");

                if (!string.IsNullOrEmpty(source.Body))
                {
                    body.AppendLine();
                    body.Append("> " + source.Body.Replace("\r\n", "\r\n> "));
                }
            }

            reply.Body = body.ToString();
            reply.IsBodyHtml = source.IsBodyHtml;

            return reply;
        }

        public static void SendReplies(IEnumerable<MailMessage> replies)
        {
            using (SmtpClient client = new SmtpClient(smtpHost, 587)) //465, 587
            {
                // Set SMTP client properties
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(FromAddress, FromPassword);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };
                // Send
                bool retry = true;
                foreach (MailMessage msg in replies)
                {
                    try
                    {
                        client.Send(msg.From.ToString(), msg.To.ToString(), msg.Subject, msg.Body);
                        retry = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception: " + ex.Message);
                        if (!retry)
                        {
                            Console.WriteLine("Failed to send email reply to " + msg.To.ToString() + '.');
                            Console.WriteLine("Exception: " + ex.Message);
                            return;
                        }

                        retry = false;
                    }
                    finally
                    {
                        msg.Dispose();
                    }
                }

                Console.WriteLine("All email replies successfully sent.");
            }
        }

        public static void Reply()
        {
            // Download unread messages from the server
            IEnumerable<MailMessage> messages = GetUnseenMessages(FromAddress, FromPassword);
            if (messages != null)
            {

                // Create message replies
                List<MailMessage> replies = new List<MailMessage>();
                foreach (MailMessage msg in messages)
                {
                    replies.Add(CreateReply(msg));
                    msg.Dispose();
                }

                // Send replies
                SendReplies(replies);
            }
            else
            {
                Console.WriteLine("No new email messages.");
            }
        }
    }
}
