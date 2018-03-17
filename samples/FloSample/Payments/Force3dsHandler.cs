using System.Threading;
using System.Threading.Tasks;
using Flo;
using OneOf;

namespace FloSample
{
    public class Force3dsHandler : IHandler<RequestPayment, OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>
    {
        public async Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>> HandleAsync(RequestPayment command, System.Func<RequestPayment, Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>> next)
        {
            var processingSettings = await GetProcessingSettings(command.MerchantId);
            if (processingSettings.Force3ds)
            {
                command.Is3ds = true;
                command.AttemptN3d = false;
            }

            return await next.Invoke(command);
        }

        Task<ProcessingSettings> GetProcessingSettings(int merchantId)
        {
            return Task.FromResult(new ProcessingSettings
            {
                Force3ds = false
            });
        }
    }
}