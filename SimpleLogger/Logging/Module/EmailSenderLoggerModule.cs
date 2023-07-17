#region

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

using SimpleLogger.Logging.Formatters;

#endregion

namespace SimpleLogger.Logging.Module
{
    public class EmailSenderLoggerModule : LoggerModule
    {
        private readonly ILoggerFormatter        _loggerFormatter;
        private readonly SmtpServerConfiguration _smtpServerConfiguration;

        private readonly string _subject;

        public EmailSenderLoggerModule(SmtpServerConfiguration smtpServerConfiguration)
            : this(smtpServerConfiguration, GenerateSubjectName())
        {
        }

        public EmailSenderLoggerModule(SmtpServerConfiguration smtpServerConfiguration, string subject)
            : this(smtpServerConfiguration, subject, new DefaultLoggerFormatter())
        {
        }

        public EmailSenderLoggerModule(SmtpServerConfiguration smtpServerConfiguration, ILoggerFormatter loggerFormatter)
            : this(smtpServerConfiguration, GenerateSubjectName(), loggerFormatter)
        {
        }

        public EmailSenderLoggerModule(SmtpServerConfiguration smtpServerConfiguration, string subject, ILoggerFormatter loggerFormatter)
        {
            _smtpServerConfiguration = smtpServerConfiguration;
            _subject                 = subject;
            _loggerFormatter         = loggerFormatter;

            Recipients = new List<string>();
        }

        public string Sender { get; set; }

        public IList<string> Recipients { get; }

        public bool EnableSsl { get; set; }

        public override string Name => "EmailSenderLoggerModule";

        public override void ExceptionLog(Exception exception)
        {
            if (string.IsNullOrEmpty(Sender) || Recipients.Count == 0)
            {
                throw new NullReferenceException("Not specified email sender and recipient. ");
            }

            string body = MakeEmailBodyFromLogHistory();

            SmtpClient client = new SmtpClient(_smtpServerConfiguration.Host, _smtpServerConfiguration.Port)
            {
                EnableSsl             = EnableSsl,
                UseDefaultCredentials = false,
                Credentials           = new NetworkCredential(_smtpServerConfiguration.UserName, _smtpServerConfiguration.Password)
            };

            foreach (string recipient in Recipients)
            {
                using (MailMessage mailMessage = new MailMessage(Sender, recipient, _subject, body))
                {
                    client.Send(mailMessage);
                }
            }
        }

        private static string GenerateSubjectName()
        {
            DateTime currentDate = DateTime.Now;
            return string.Format("SimpleLogger {0} {1}", currentDate.ToShortDateString(), currentDate.ToShortTimeString());
        }

        private string MakeEmailBodyFromLogHistory()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Simple logger - email module");

            foreach (LogMessage logMessage in Logger.Messages)
            {
                stringBuilder.AppendLine(_loggerFormatter.ApplyFormat(logMessage));
            }

            return stringBuilder.ToString();
        }
    }
}