using System.Threading;
using System.Threading.Tasks;
using Flo;
using OneOf;

namespace FloSample
{
    /// <summary>
    /// Example of a projecting handler, from the payments pipeline into the Risk pipeline
    /// </summary>
    public class PreAuthRiskCheckHandler : IHandler<RequestPayment, OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>
    {
        public async Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>> HandleAsync(RequestPayment command, System.Func<RequestPayment, System.Threading.Tasks.Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>> next, CancellationToken token)
        {
            var merchantAddress = await GetMerchantAddress(command.MerchantId);

            var riskContext = new RiskContext
            {
                Cardholder = command.Cardholder,
                MerchantId = command.MerchantId,
                Amount = command.Amount,
                Currency = command.Currency,
                MerchantCountry = merchantAddress.Country
            };

            // Now project to the Risk pipeline

            var riskPipeline = RiskPipeline.Build();
            riskContext = await riskPipeline.Invoke(riskContext);

            if (!riskContext.Result.Passed)
                return RiskDeclined(riskContext.Result);

            if (riskContext.Result.Requires3ds)
                command.AttemptN3d = false;

            // Otherwise, risk check passed, proceed to next handler
            return await next.Invoke(command);
        }

        PaymentCreated RiskDeclined(RiskAssessment riskAssessment)
        {
            // At this point we would need access to the payment object
            // in order to persist this data
            // How does this get accessed - from the RequestPayment command?
            return new PaymentCreated { ResponseCode = "40401" };
        }

        Task<Address> GetMerchantAddress(int merchantId)
        {
            return Task.FromResult(new Address());
        }
    }
}