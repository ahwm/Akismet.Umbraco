using Akismet.Net;
using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using Umbraco.Forms.Core.Enums;
using System.Net.Mail;
using System.Text;
using Umbraco.Forms.Core.Persistence.Dtos;

namespace Akismet.Umbraco.Forms.Workflows
{
    public class AkismetWorkflow : WorkflowType
    {
        [Setting("User's Email Field", Description = "Field alias that will contain the submitter's email address")]
        public string UserEmail { get; set; }

        [Setting("User's Name Field", Description = "Field alias that will contain the submitter's name")]
        public string UserName { get; set; }

        [Setting("User's Comment Field", Description = "Field alias that will contain the submitter's comment")]
        public string UserComment { get; set; }

        [Setting("Email", Description = "Enter the receiver email")]
        public string Emails { get; set; }

        [Setting("Sender Email", Description = "Enter the sender email (if blank it will use the settings from the web.config file)")]
        public string SenderEmail { get; set; }

        [Setting("Subject", Description = "Enter the subject")]
        public string Subject { get; set; } = "The Form '{form_name}' was submitted";

        [Setting("Message", Description = "Enter the intro message", View = "textarea")]
        public string Message { get; set; } = "The Form '{form_name}' was submitted";

        private readonly AkismetService AkismetService;

        public AkismetWorkflow(AkismetService akismetService)
        {
            Id = new Guid("bbad4e95-7b5c-4320-9855-dc514fd74979");
            Name = "Send email with spam check";
            Description = "Check message for spam content before sending to recipient(s)";
            Icon = "icon-message";

            AkismetService = akismetService;
        }

        public override WorkflowExecutionStatus Execute(Record record, RecordEventArgs e)
        {
            var commentField = record.GetRecordFieldByAlias(UserComment);
            var nameField = record.GetRecordFieldByAlias(UserName);
            var emailField = record.GetRecordFieldByAlias(UserEmail);
            string commentValue = "", emailValue = "", nameValue = "";

            if (commentField != null)
            {
                if (commentField.HasValue())
                {
                    commentValue = commentField.Values[0].ToString();
                }
            }
            if (nameField != null)
            {
                if (nameField.HasValue())
                {
                    nameValue = nameField.Values[0].ToString();
                }
            }
            if (emailField != null)
            {
                if (emailField.HasValue())
                {
                    emailValue = emailField.Values[0].ToString();
                }
            }

            HttpContext ctx = HttpContext.Current;

            string ip = ctx.Request.Headers["CF-Connecting-IP"] ?? ctx.Request.UserHostAddress;
            if (String.IsNullOrWhiteSpace(ip))
                ip = ctx.Request.ServerVariables["REMOTE_HOST"];
            AkismetComment comment = new AkismetComment
            {
                CommentAuthor = nameValue,
                CommentAuthorEmail = emailValue,
                CommentContent = commentValue,
                CommentType = AkismentCommentType.ContactForm,
                UserAgent = ctx.Request.UserAgent,
                Referrer = ctx.Request.UrlReferrer.ToString(),
                UserIp = ip
            };
            var isValid = AkismetService.CheckComment(comment, true);

            StringBuilder body = new StringBuilder($"<p>{Message}</p><dl>");
            foreach (var field in record.RecordFields)
            {
                body.AppendLine($"<dt><strong>{field.Value.Field.Caption}</strong></dt><dd>{field.Value.ValuesAsString()}</dd>");
            }
            body.AppendLine("</dl>");

            if (isValid)
            {
                string[] emails = Emails.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                using (MailMessage m = new MailMessage())
                {
                    foreach (string em in emails)
                        m.To.Add(em.Trim());
                    if (!String.IsNullOrWhiteSpace(SenderEmail))
                        m.From = new MailAddress(SenderEmail);
                    m.Subject = Subject.Replace("{form_name}", e.Form.Name);
                    m.Body = body.ToString();
                    m.IsBodyHtml = true;

                    using (SmtpClient c = new SmtpClient())
                    {
                        try
                        {
                            c.Send(m);
                            return WorkflowExecutionStatus.Completed;
                        }
                        catch
                        {
                            return WorkflowExecutionStatus.Failed;
                        }
                    }
                }
            }
            else
            {
                return WorkflowExecutionStatus.Completed;
            }
        }

        public override List<Exception> ValidateSettings()
        {
            List<Exception> exceptionList = new List<Exception>();

            if (String.IsNullOrWhiteSpace(UserComment))
                exceptionList.Add(new Exception("'User Comment' has not been set"));

            if (String.IsNullOrWhiteSpace(Subject))
                exceptionList.Add(new Exception("'Subject' has not been set"));

            return exceptionList;
        }
    }
}