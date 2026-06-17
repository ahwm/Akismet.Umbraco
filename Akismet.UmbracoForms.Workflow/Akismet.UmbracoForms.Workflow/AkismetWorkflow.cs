using Akismet.Net;
using Akismet.Umbraco;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using Umbraco.Forms.Core.Enums;

namespace Akismet.Umbraco.Forms.Workflows
{
    public class AkismetWorkflow : WorkflowType
    {
        [Setting("User's Email Field", Description = "Field alias that will contain the submitter's email address", View = "TextField")]
        public string UserEmail { get; set; }

        [Setting("User's Name Field", Description = "Field alias that will contain the submitter's name", View = "TextField")]
        public string UserName { get; set; }

        [Setting("User's Comment Field", Description = "Field alias that will contain the submitter's comment", View = "TextField")]
        public string UserComment { get; set; }

        [Setting("Email", Description = "Enter the receiver email", View = "TextField")]
        public string Emails { get; set; }

        [Setting("Sender Email", Description = "Enter the sender email (if blank it will use the SMTP settings from configuration)", View = "TextField")]
        public string SenderEmail { get; set; }

        [Setting("Subject", Description = "Enter the subject", View = "TextField")]
        public string Subject { get; set; } = "The Form '{form_name}' was submitted";

        [Setting("Message", Description = "Enter the intro message", View = "Textarea")]
        public string Message { get; set; } = "The Form '{form_name}' was submitted";

        private readonly AkismetService _akismetService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailSender _emailSender;

        public AkismetWorkflow(AkismetService akismetService, IHttpContextAccessor httpContextAccessor, IEmailSender emailSender)
        {
            Id = new Guid("bbad4e95-7b5c-4320-9855-dc514fd74979");
            Name = "Send email with spam check";
            Description = "Check message for spam content before sending to recipient(s)";
            Icon = "icon-message";

            _akismetService = akismetService;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
        }

        public override async Task<WorkflowExecutionStatus> ExecuteAsync(WorkflowExecutionContext context)
        {
            var record = context.Record;

            var commentField = record.GetRecordFieldByAlias(UserComment);
            var nameField = record.GetRecordFieldByAlias(UserName);
            var emailField = record.GetRecordFieldByAlias(UserEmail);
            string commentValue = "", emailValue = "", nameValue = "";

            if (commentField != null && commentField.HasValue())
                commentValue = commentField.Values[0].ToString();
            if (nameField != null && nameField.HasValue())
                nameValue = nameField.Values[0].ToString();
            if (emailField != null && emailField.HasValue())
                emailValue = emailField.Values[0].ToString();

            HttpContext ctx = _httpContextAccessor.HttpContext;

            string ip = ctx?.Request.Headers["CF-Connecting-IP"].ToString();
            if (String.IsNullOrWhiteSpace(ip))
                ip = ctx?.Connection.RemoteIpAddress?.ToString();

            AkismetComment comment = new AkismetComment
            {
                CommentAuthor = nameValue,
                CommentAuthorEmail = emailValue,
                CommentContent = commentValue,
                CommentType = AkismentCommentType.ContactForm,
                UserAgent = ctx?.Request.Headers["User-Agent"].ToString(),
                Referrer = ctx?.Request.Headers["Referer"].ToString(),
                UserIp = ip
            };
            var isValid = await _akismetService.CheckCommentAsync(comment, true);

            StringBuilder body = new StringBuilder($"<p>{Message}</p><dl>");
            foreach (var field in record.RecordFields)
            {
                body.AppendLine($"<dt><strong>{field.Value.Field.Caption}</strong></dt><dd>{field.Value.ValuesAsString()}</dd>");
            }
            body.AppendLine("</dl>");

            if (!isValid)
                return WorkflowExecutionStatus.Completed;

            string[] emails = Emails.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < emails.Length; i++)
                emails[i] = emails[i].Trim();

            var message = new EmailMessage(
                String.IsNullOrWhiteSpace(SenderEmail) ? null : SenderEmail,
                emails,
                null,
                null,
                null,
                Subject.Replace("{form_name}", context.Form.Name),
                body.ToString(),
                true,
                null);

            try
            {
                await _emailSender.SendAsync(message, "Akismet");
                return WorkflowExecutionStatus.Completed;
            }
            catch
            {
                return WorkflowExecutionStatus.Failed;
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
