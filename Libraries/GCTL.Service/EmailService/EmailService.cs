using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using GCTL.Core.ViewModels.Email;

namespace GCTL.Service.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using (var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port))
            {
                client.Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password);
                client.EnableSsl = _emailSettings.EnableSsl;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.MailFrom),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
        }

        // Generate Leave Request Email
        public string GenerateLeaveRequestEmail(string employeeName, string leaveType, string startDate, string endDate, string reason)
        {
            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .container {{ background: #f9f9f9; padding: 20px; border-radius: 8px; }}
                h1 {{ color: #007bff; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h1>Leave Request Submitted</h1>
                <p>Dear {employeeName},</p>
                <p>We have received your leave request:</p>
                <ul>
                    <li><strong>Leave Type:</strong> {leaveType}</li>
                    <li><strong>Start Date:</strong> {startDate}</li>
                    <li><strong>End Date:</strong> {endDate}</li>
                    <li><strong>Reason:</strong> {reason}</li>
                </ul>
                <p>Thank you,</p>
                <p>The HR Team</p>
            </div>
        </body>
        </html>";
        }

        // Generate Leave Approval Email
        public string GenerateLeaveApprovalEmail(string employeeName, string leaveType, string startDate, string endDate)
        {
            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .container {{ background: #f9f9f9; padding: 20px; border-radius: 8px; }}
                h1 {{ color: #28a745; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h1>Leave Request Approved</h1>
                <p>Dear {employeeName},</p>
                <p>Your leave request has been approved:</p>
                <ul>
                    <li><strong>Leave Type:</strong> {leaveType}</li>
                    <li><strong>Start Date:</strong> {startDate}</li>
                    <li><strong>End Date:</strong> {endDate}</li>
                </ul>
                <p>Enjoy your leave!</p>
                <p>The HR Team</p>
            </div>
        </body>
        </html>";
        }

        // Generate Leave Rejection Email
        public string GenerateLeaveRejectionEmail(string employeeName, string leaveType, string rejectionReason)
        {
            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .container {{ background: #f9f9f9; padding: 20px; border-radius: 8px; }}
                h1 {{ color: #dc3545; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h1>Leave Request Rejected</h1>
                <p>Dear {employeeName},</p>
                <p>We regret to inform you that your leave request has been rejected:</p>
                <ul>
                    <li><strong>Leave Type:</strong> {leaveType}</li>
                    <li><strong>Reason:</strong> {rejectionReason}</li>
                </ul>
                <p>Please contact your manager for more details.</p>
                <p>The HR Team</p>
            </div>
        </body>
        </html>";
        }
    }
}
