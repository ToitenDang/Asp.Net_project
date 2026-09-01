using IdentityService.Events;
using System.Threading.Channels;

namespace IdentityService.Workers
{
    public class EmailQueue
    {
        // Tạo một hàng đợi không giới hạn dung lượng
        private readonly Channel<SendEmailEvent> _channel = Channel.CreateUnbounded<SendEmailEvent>();

        // Hàm để Controller đẩy việc vào hàng đợi[cite: 1]
        public async ValueTask QueueEmailAsync(SendEmailEvent emailEvent)
        {
            await _channel.Writer.WriteAsync(emailEvent);
        }

        // Hàm để Worker ngầm lấy việc ra xử lý[cite: 1]
        public IAsyncEnumerable<SendEmailEvent> DequeueEmailsAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }
    }
}