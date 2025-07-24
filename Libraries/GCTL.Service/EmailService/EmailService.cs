using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using GCTL.Core.ViewModels.Email;
using Microsoft.Extensions.Hosting;

namespace GCTL.Service.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly SemaphoreSlim _semaphore;
        private readonly IHostEnvironment hostEnvironment;


        public EmailService(IOptions<EmailSettings> emailSettings, IHostEnvironment hostEnvironment)
        {
            _emailSettings = emailSettings.Value;
            _semaphore = new SemaphoreSlim(5);
            this.hostEnvironment = hostEnvironment;
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

        public async Task SendSingleEmailAsync(string to, string subject, string body,string attachmentPath = null, string attachmentname = null)
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

               
                if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath)) 
                { 
                    var attachment = new Attachment(attachmentPath);
                    
                    attachment.Name = attachmentname;   

                    mailMessage.Attachments.Add(attachment);
                }

                await client.SendMailAsync(mailMessage);
            }
        }

        public async Task SendBatchEmailsAsync(List<EmailDataDto> request, int batchSize = 10, int delayBetweenBatches = 1000)
        {
            var batches = request
                .Select((email, index) => new { email, index })
                .GroupBy(x => x.index / batchSize)
                .Select(group => group.Select(x => x.email).ToList())
                .ToList();

            foreach (var batch in batches) 
            {
                var batchTasks = batch.Select(async request =>
                {
                    try
                    {
                        
                        await SendSingleEmailAsync(request.To, request.Subject, request.Body, request.AttachmentPath, request.AttachmentName);
                        return new EmailResult { Email = request.To, isSuccess = true };
                    }
                    catch (Exception ex)
                    {
                        return new EmailResult { Email=request.To, isSuccess = false, Error = ex.Message };
                    }
                });

                await Task.WhenAll(batchTasks);

                if (delayBetweenBatches > 0) 
                {
                    await Task.Delay(delayBetweenBatches);
                }
            }
        }

        //public async Task<bool> SendBulkEmailsAsync(List<EmailDataDto> emails)
        //{
        //    if (emails.Count == 0) return false;

        //    int batchNum = 1;

        //    for(int i = 0; i<emails.Count; i += 10)
        //    {
        //        var batch = emails.Skip(i).Take(10).ToList();

        //        var batchTasks = batch.Select(async email =>{
        //            var success = await SendEmailWithRetry(email.To, email.Subject, email.Body);
        //            return success;
        //        });

        //        try
        //        {
        //            var results = await Task.WhenAll(batchTasks);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Encountered error: {ex.Message}");
        //        }

        //        if(i+ 10 < emails.Count)
        //        {
        //            await Task.Delay(2000);
        //        }

        //        batchNum++;
        //    }
        //    return true;
        //}

        //public async Task<bool> SendEmailWithRetry(string to, string subject, string body)
        //{
        //    await _emailSemaphore.WaitAsync();

        //    try
        //    {
        //        lock (_lockObj)
        //        {
        //            var timeSinceLastEmail = DateTime.Now - _lastSent;
        //            var minDelay = TimeSpan.FromMilliseconds(500);

        //            if (timeSinceLastEmail < minDelay)
        //            {
        //                var delayNeeded = minDelay - timeSinceLastEmail;
        //                Thread.Sleep(delayNeeded);
        //            }
        //            _lastSent = DateTime.Now;
        //        }

        //        for (int attempt = 1; attempt <= 3; attempt++)
        //        {
        //            try
        //            {
        //                await SendEmailAsync(to, subject, body);
        //                return true;
        //            }
        //            catch (SmtpException ex) when (attempt < 3)
        //            {
        //                Console.WriteLine($"Email send attempt {attempt} failed for {to}: {ex.Message}");

        //                // Exponential backoff: wait longer between retries
        //                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // 2s, 4s, 8s...
        //                await Task.Delay(delay);
        //            }
        //            catch (Exception ex) when (attempt < 3)
        //            {
        //                Console.WriteLine($"Email send attempt {attempt} failed for {to}: {ex.Message}");
        //                await Task.Delay(TimeSpan.FromSeconds(2 * attempt));
        //            }
        //        }
        //        return false; ;
        //    }
        //    finally
        //    {
        //        _emailSemaphore.Release();
        //    }
        //}
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
