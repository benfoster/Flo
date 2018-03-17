using System;
using System.Threading;
using System.Threading.Tasks;
using Flo;
using OneOf;

namespace FloSample
{
    public class CardVaultHandler : IHandler<RequestPayment, OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>
    {
        public async Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>> HandleAsync(RequestPayment command, Func<RequestPayment, Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>> next, CancellationToken token)
        {
            // Could be used to load the card details on the way and set on the context
            
            var result = await next.Invoke(command);
            if (result.IsT0 && result.AsT0.Approved)
            {
                // Store card details
            }

            return result;
        }
    }
}