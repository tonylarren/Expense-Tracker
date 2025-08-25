using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Do nothing
            return Task.CompletedTask;
        }
    }
}