using System;
using System.Threading;
using System.Threading.Tasks;
using Flo;

namespace FloSample
{
    public class MerchantValidator : IHandler<RequestPayment, ValidationResult>
    {
        public async Task<ValidationResult> HandleAsync(
            RequestPayment command, 
            Func<RequestPayment, Task<ValidationResult>> next)
        {
            if (command.MerchantId <= 0)
                return new ValidationResult
                {
                    ErrorCode = "merchant_invalid"
                };
            
            return await next.Invoke(command);
        }
    }
}