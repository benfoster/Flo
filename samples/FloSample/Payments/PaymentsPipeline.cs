using System;
using System.Threading.Tasks;
using Flo;
using OneOf;

namespace FloSample
{
    public class PaymentsPipeline
    {
        public static Func<RequestPayment, Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>> Build()
        {
            return Pipeline.Build<RequestPayment, OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>(cfg =>
                 cfg
                    .Add<CardVaultHandler>() // Loads cards details on the way in, stores new cards on the way out
                    .Add<PaymentValidationHandler>() // Validates the payment request
                    .Add<PreAuthRiskCheckHandler>() // Performs pre auth risk checks
                    .Add<Force3dsHandler>() // Forces 3ds and disables attemptn3ds if set in processing settings
                    .When(command => command.Is3ds, inner => 
                        inner.Add<ThreeDsHandler>() // Verifies the enrollment and handles AttemptN3D is enrollment fails
                    )
                    .Add<CaptureHandler>() // Captures the payment on the way out if ProcessingHandler result succeeds
                    .Add<ProcessingHandler>() // Final handler, processes the payment
            );
        }
    }
}