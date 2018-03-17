using System;
using System.Threading;
using System.Threading.Tasks;
using Flo;
using OneOf;

namespace FloSample
{
    public class CaptureHandler : IHandler<RequestPayment, OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>
    {
        public async Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>> HandleAsync(RequestPayment command, Func<RequestPayment, Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>> next)
        {
            var result = await next.Invoke(command);

            if (command.Capture && result.IsT0 && result.AsT0.Approved)
            {
                // Queue capture
            }

            return result;
        }
    }
}