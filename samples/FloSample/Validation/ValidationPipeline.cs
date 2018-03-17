using System;
using System.Threading.Tasks;
using Flo;

namespace FloSample
{
    public class ValidationPipeline
    {
        public static Func<RequestPayment, Task<ValidationResult>> Build()
        {
            return Pipeline.Build<RequestPayment, ValidationResult>(cfg =>
                cfg.Add<MerchantValidator>()
                .Final(s => Task.FromResult(new ValidationResult { IsValid = true }))
            );
        }
    }
}