using GCTL.Core.ViewModels.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.EmailService
{
    public interface IEmailService 
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendBatchEmailsAsync(List<EmailDataDto> request, int batchSize = 10, int delayBetweenBatches = 1000);
        //Task<bool> SendBulkEmailsAsync(List<EmailDataDto> emails);
        //Task<bool> SendEmailWithRetry(string to, string subject, string body);

    }
}
