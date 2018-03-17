using System;
using System.Threading;
using System.Threading.Tasks;
using Flo;
using OneOf;

namespace FloSample
{
    public class ThreeDsHandler : IHandler<RequestPayment, OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>
    {
        public async Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>> HandleAsync(RequestPayment command, Func<RequestPayment, Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>> next, CancellationToken token)
        {
            var enrollmentResult = await VerifyEnrollement(command.MerchantId, command.Currency);

            if (!enrollmentResult.Enrolled)
            {
                if (command.AttemptN3d) 
                    return await next.Invoke(command);

                // Otherwise, payment can't be processed
                return new PaymentFailed { Errors = new[] { "3ds_not_enrolled" } };
            }

            return Accepted(enrollmentResult);
        }

        Task<EnrollmentResult> VerifyEnrollement(int merchant, string currency)
        {
            return Task.FromResult(new EnrollmentResult
            {
                Enrolled = true,
                RedirectUrl = "https://localhost.com/acs"
            });
        }

        PaymentAccepted Accepted(EnrollmentResult enrollmentResult)
        {
            return new PaymentAccepted
            {
                RedirectUrl = enrollmentResult.RedirectUrl
            };
        }
    }
}