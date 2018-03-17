using System.Threading;
using System.Threading.Tasks;
using Flo;
using OneOf;

namespace FloSample
{
    /// <summary>
    /// Payments handler that runs the payment request through a validation pipeline
    /// </summary>
    public class PaymentValidationHandler : IHandler<RequestPayment, OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>
    {
        public async Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>> HandleAsync(RequestPayment command, System.Func<RequestPayment, Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>> next)
        {
            var validationPipeline = ValidationPipeline.Build();
            
            var validationResult = await validationPipeline.Invoke(command);

            if (!validationResult.IsValid)
                return new PaymentFailed { Errors = new[] { validationResult.ErrorCode } };  

            return await next.Invoke(command);
        }
    }
}