using IdentityService.Services;
using IdentityService.Services.IService;

namespace IdentityService.Workers
{
    public class EmailWorker : BackgroundService
    {
        private readonly EmailQueue _emailQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailWorker> _logger;

        public EmailWorker(EmailQueue emailQueue, IServiceProvider serviceProvider, ILogger<EmailWorker> logger)
        {
            _emailQueue = emailQueue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Worker đã khởi động...");

            await foreach (var emailEvent in _emailQueue.DequeueEmailsAsync(stoppingToken))
            {
                // Vì BackgroundService là Singleton nhưng DbContext/EmailService thường là Scoped,
                // ta cần tạo một Scope riêng để gọi dịch vụ an toàn.
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                try
                {
                    await emailService.SendEmailAsync(emailEvent.ToEmail, emailEvent.Subject, emailEvent.Body);
                    _logger.LogInformation($"Đã gửi email thành công tới: {emailEvent.ToEmail}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi gửi email tới {emailEvent.ToEmail}");
                }
            }
        }
    }
}