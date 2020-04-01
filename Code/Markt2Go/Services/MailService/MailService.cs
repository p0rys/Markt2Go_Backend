using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Configuration;

using MimeKit;
using MailKit.Net.Smtp;

namespace Markt2Go.Services.MailService
{
    public class MailService : IMailService
    {
        IConfiguration _configuration;
        public MailService(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _configuration = configuration;
        }

        public async Task SendReservationConfirmation(string recipientMail, Dictionary<string, string> placeholders)
        {            
            if (!placeholders.ContainsKey("{recipientName}"))
                throw new ArgumentNullException("Placeholder '{recipientName}' is mandantory to send mails.");

            // create subject and body
            var body = InsertPlaceholder(_configuration["Mail:ReservationConfirmation:Body"], placeholders);
            var subject = InsertPlaceholder(_configuration["Mail:ReservationConfirmation:Subject"], placeholders);
            // create message
            var message = CreateMessage(recipientMail, placeholders["{recipientName}"], subject, body);
            // send 
            await Send(message);
        }
        public async Task SendReservationAccepted(string recipientMail, Dictionary<string, string> placeholders)
        {            
            if (!placeholders.ContainsKey("{recipientName}"))
                throw new ArgumentNullException("Placeholder '{recipientName}' is mandantory to send mails.");

            // create subject and body
            var body = InsertPlaceholder(_configuration["Mail:ReservationAccepted:Body"], placeholders);
            var subject = InsertPlaceholder(_configuration["Mail:ReservationAccepted:Subject"], placeholders);
            // create message
            var message = CreateMessage(recipientMail, placeholders["{recipientName}"], subject, body);
            // send 
            await Send(message);
        }
        public async Task SendReservationDeclined(string recipientMail, Dictionary<string, string> placeholders)
        {            
            if (!placeholders.ContainsKey("{recipientName}"))
                throw new ArgumentNullException("Placeholder '{recipientName}' is mandantory to send mails.");

            // create subject and body
            var body = InsertPlaceholder(_configuration["Mail:ReservationDeclined:Body"], placeholders);
            var subject = InsertPlaceholder(_configuration["Mail:ReservationDeclined:Subject"], placeholders);
            // create message
            var message = CreateMessage(recipientMail, placeholders["{recipientName}"], subject, body);
            // send 
            await Send(message);
        }
        public async Task SendReservationPacked(string recipientMail, Dictionary<string, string> placeholders)
        {
            if (!placeholders.ContainsKey("{recipientName}"))
                throw new ArgumentNullException("Placeholder '{recipientName}' is mandantory to send mails.");

            // create subject and body
            var body = InsertPlaceholder(_configuration["Mail:ReservationPacked:Body"], placeholders);
            var subject = InsertPlaceholder(_configuration["Mail:ReservationPacked:Subject"], placeholders);
            // create message
            var message = CreateMessage(recipientMail, placeholders["{recipientName}"], subject, body);
            // send 
            await Send(message);
        }


        private async Task Send(MimeMessage message)
        {
            var smtpAddress = _configuration["SMTP:Address"];
            var smtpPort = Convert.ToInt32(_configuration["SMTP:Port"]);
            var smtpUseSSL = Convert.ToBoolean(_configuration["SMTP:UseSSL"]);
            var smtpUser = _configuration["SMTP:User"];
            var smtpPassword = _configuration["SMTP:Password"];

            using (var client = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(smtpAddress, smtpPort, smtpUseSSL);
                await client.AuthenticateAsync(smtpUser, smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
        private MimeMessage CreateMessage(string recipientMail, string recipientName, string subject, string body)
        {
            var message = new MimeMessage();

            // add sender
            message.From.Add(
                new MailboxAddress(
                    _configuration["Mail:Sender"],
                    _configuration["Mail:SenderAddress"]
                ));

            // add recipient
            message.To.Add(
                new MailboxAddress(
                    recipientName,
                    recipientMail
                ));

            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            return message;
        }
        private string InsertPlaceholder(string text, IDictionary<string, string> placeholders)
        {
            string patternIf = @"(#IF)\{(.+?)\}\((.+?)\)";
            var match = Regex.Match(text, patternIf, RegexOptions.Singleline);
            if (match.Success && match.Groups.Count == 4)
            {
                var param = $"{{{match.Groups[2].Value}}}";
                if (placeholders.ContainsKey(param) && placeholders[param].Trim() != string.Empty)
                {
                    // replace query in initial text
                    text = text.Replace(match.Value, match.Groups[3].Value);
                }
                else
                {
                    // delete area if not needed
                    text = text.Replace(match.Value, string.Empty);
                }
            }

            var finishedText = text;
            foreach (var placeholder in placeholders)
            {
                // replace all available placeholders
                finishedText = finishedText.Replace(placeholder.Key, placeholder.Value);
            }
            return finishedText;
        }
    }
}