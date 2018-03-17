using System;
using System.Threading;
using System.Threading.Tasks;
using Flo;
using OneOf;

namespace FloSample
{
    public class ProcessingHandler : IHandler<RequestPayment, OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>
    {
        public async Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>> HandleAsync(RequestPayment command, System.Func<RequestPayment, Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>>> next)
        {
            var paymentId = Guid.NewGuid();
            var transaction = await Process(command, paymentId);

            return new PaymentCreated
            {
                Id = paymentId,
                ResponseCode = transaction.ResponseCode,
                ResponseSummary = transaction.ResponseSummary,
                Approved = transaction.Approved
            };
        }

        Task<Transaction> Process(RequestPayment command, Guid paymentId)
        {
            return Task.FromResult(new Transaction());
        }
    }
}