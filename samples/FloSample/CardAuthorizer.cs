using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OneOf;
using Flo;

namespace FloSample
{
    class CardAuthorizer
    {
        private readonly IMerchantValidator _merchantValidator;
        private readonly ICurrencyValidator _currencyValidator;
        private readonly IFinancialInstitutionValidator _finValidator;
        private readonly Func<RequestPayment, Task<ValidationResult>> _validator;


        public CardAuthorizer(
            IMerchantValidator merchantValidator, 
            ICurrencyValidator currencyValidator,
            IFinancialInstitutionValidator finValidator,
            Func<RequestPayment, Task<ValidationResult>> validator = null)
        {
            _merchantValidator = merchantValidator;
            _currencyValidator = currencyValidator;
            _finValidator = finValidator;

            _validator = validator ?? ValidationPipeline.Build();
        }

        public async Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>> AuthorizeAsync2(RequestPayment command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            // Enriching example     with Pipeline<T> - requires mutability


            var validationResult = await _validator.Invoke(command);

            if (!validationResult.IsValid)
                return Failed(validationResult.ErrorCode);

            // Validation
            // if (!await _merchantValidator.IsValid(command.MerchantId))
            //     return Failed("merchant_invalid");

            // // New regulatory requirement for financial institutions
            // if (!await _finValidator.IsValid(command.MerchantId, command.RecipientName))
            //     return Failed("recipient_required");

            if (!await _currencyValidator.IsValid(command.Currency))
                return Failed("currency_invalid");

            // Risk Check
            var merchantAddress = await GetMerchantAddress(command.MerchantId);

            var assessRisk = new AssessRisk
            {
                Cardholder = command.Cardholder,
                MerchantId = command.MerchantId,
                Amount = command.Amount,
                Currency = command.Currency,
                MerchantCountry = merchantAddress.Country
            };

            var riskAssessment = await PreAuthRiskAssessment(assessRisk);

            if (!riskAssessment.Passed)
                return RiskDeclined(riskAssessment);

            if (riskAssessment.Requires3ds)
                command.AttemptN3d = false;

            if (command.Is3ds)
            {
                var enrollmentResult = await VerifyEnrollement(command.MerchantId, command.Currency);

                if (!enrollmentResult.Enrolled)
                {
                    if (!command.AttemptN3d || Force3dsEnabled(command.MerchantId)) // can be done with a handler to set the charge mode and disable attempt n3ds
                        return Failed("3ds_not_enrolled");
                }


                return Accepted(enrollmentResult);
            }

            var paymentId = Guid.NewGuid();
            var transaction = await Process(command, paymentId);

            if (transaction.Approved)
            {
                // Only store the card if auth was approved
                await StoreCardInVault(command.CardNumber, command.ExpiryMonth, command.ExpiryYear);

                if (command.Capture)
                    await QueueCapture(paymentId);
            }

            return new PaymentCreated
            {
                Id = paymentId,
                ResponseCode = transaction.ResponseCode,
                ResponseSummary = transaction.ResponseSummary
            };
        }

        public async Task<OneOf<PaymentCreated, PaymentAccepted, PaymentFailed>> AuthorizeAsync(RequestPayment command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            // Validation
            if (!await _merchantValidator.IsValid(command.MerchantId))
                return Failed("merchant_invalid");

            // // New regulatory requirement for financial institutions
            if (!await _finValidator.IsValid(command.MerchantId, command.RecipientName))
                return Failed("recipient_required");

            if (!await _currencyValidator.IsValid(command.Currency))
                return Failed("currency_invalid");

            // Risk Check
            var merchantAddress = await GetMerchantAddress(command.MerchantId);

            var assessRisk = new AssessRisk
            {
                Cardholder = command.Cardholder,
                MerchantId = command.MerchantId,
                Amount = command.Amount,
                Currency = command.Currency,
                MerchantCountry = merchantAddress.Country
            };

            var riskAssessment = await PreAuthRiskAssessment(assessRisk);

            if (!riskAssessment.Passed)
                return RiskDeclined(riskAssessment);

            if (riskAssessment.Requires3ds)
                command.AttemptN3d = false;

            if (command.Is3ds)
            {
                var enrollmentResult = await VerifyEnrollement(command.MerchantId, command.Currency);

                if (!enrollmentResult.Enrolled)
                {
                    if (!command.AttemptN3d || Force3dsEnabled(command.MerchantId)) // can be done with a handler to set the charge mode and disable attempt n3ds
                        return Failed("3ds_not_enrolled");
                }


                return Accepted(enrollmentResult);
            }

            var paymentId = Guid.NewGuid();
            var transaction = await Process(command, paymentId);

            if (transaction.Approved)
            {
                // Only store the card if auth was approved
                await StoreCardInVault(command.CardNumber, command.ExpiryMonth, command.ExpiryYear);

                if (command.Capture)
                    await QueueCapture(paymentId);
            }

            return new PaymentCreated
            {
                Id = paymentId,
                ResponseCode = transaction.ResponseCode,
                ResponseSummary = transaction.ResponseSummary
            };
        }

        private bool Force3dsEnabled(int merchantId)
        {
            return false;
        }

        Task<Transaction> Process(RequestPayment command, Guid paymentId)
        {
            return Task.FromResult(new Transaction());
        }

        Task<RiskAssessment> PreAuthRiskAssessment(AssessRisk assessRisk)
        {
            return Task.FromResult(new RiskAssessment
            {

            });
        }
        Task<Address> GetMerchantAddress(int merchantId)
        {
            return Task.FromResult(new Address());
        }

        Task StoreCardInVault(string cardNumber, int expiryMonth, int expiryYear)
        {
            return Task.CompletedTask;
        }

        Task QueueCapture(Guid paymentId)
        {
            return Task.CompletedTask;
        }

        Task<EnrollmentResult> VerifyEnrollement(int merchant, string currency)
        {
            return Task.FromResult(new EnrollmentResult
            {
                Enrolled = true,
                RedirectUrl = "https://localhost.com/acs"
            });
        }

        PaymentFailed Failed(string error)
        {
            return new PaymentFailed { Errors = new[] { error } };
        }

        PaymentAccepted Accepted(EnrollmentResult enrollmentResult)
        {
            return new PaymentAccepted
            {
                RedirectUrl = enrollmentResult.RedirectUrl
            };
        }

        PaymentCreated RiskDeclined(RiskAssessment riskAssessment)
        {
            return new PaymentCreated();
        }

    }

    public class RequestPayment
    {
        public int MerchantId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string CardNumber { get; set; }
        public string Cardholder { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public bool Is3ds { get; set; }
        public bool AttemptN3d { get; set; }
        public bool Capture { get; set; }
        public string RecipientName { get; set; }
    }

    class PaymentCreated
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseSummary { get; set; }
    }

    public class Transaction
    {
        public decimal Amount { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseSummary { get; set; }
        public string AcquirerResponseCode { get; set; }
        public string AuthCode { get; set; }
        public bool Approved { get; set; }
    }

    class PaymentAccepted
    {
        public Guid Id { get; set; }
        public string RedirectUrl { get; set; }
    }

    class PaymentFailed
    {
        public IEnumerable<string> Errors { get; set; }
    }

    public class EnrollmentResult
    {
        public bool Enrolled { get; set; }
        public string RedirectUrl { get; set; }
    }

    class AssessRisk
    {
        public int MerchantId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string Cardholder { get; set; }
        public string CardNumber { get; set; }
        public string MerchantCountry { get; set; }
    }

    class RiskAssessment
    {
        public RiskAssessment()
        {
            RiskChecks = new Dictionary<string, bool>();
        }

        public Dictionary<string, bool> RiskChecks { get; set; }
        public bool Requires3ds { get; set; }
        public bool Passed => RiskChecks.Any(c => !c.Value);
    }

    class Address
    {
        public string Country { get; set; }
    }
}