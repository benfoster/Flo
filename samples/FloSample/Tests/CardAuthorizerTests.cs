using System;
using System.Threading.Tasks;
using Moq;
using NSpec;
using OneOf;
using Shouldly;

namespace FloSample.Tests
{
    class describe_card_authorizer : nspec
    {
        Mock<IMerchantValidator> merchantValidator;
        Mock<ICurrencyValidator> currencyValidator;
        Mock<IFinancialInstitutionValidator> finValidator;
        ValidationResult validationResult;

        CardAuthorizer authorizer;

        RequestPayment command;
        OneOf<PaymentCreated, PaymentAccepted, PaymentFailed> response;

        void before_each()
        {
            merchantValidator = new Mock<IMerchantValidator>();
            currencyValidator = new Mock<ICurrencyValidator>();
            finValidator = new Mock<IFinancialInstitutionValidator>();
            validationResult = new ValidationResult();
            Func<RequestPayment, Task<ValidationResult>> validator = c => Task.FromResult(validationResult);

            authorizer = new CardAuthorizer(merchantValidator.Object, currencyValidator.Object, finValidator.Object, validator);

            command = new RequestPayment
            {
                MerchantId = 10000
            };

            merchantValidator
                .Setup(x => x.IsValid(command.MerchantId))
                .ReturnsAsync(true);
        }

        void when_authorizing()
        {
            //actAsync = async () => response = await authorizer.AuthorizeAsync(command);
            actAsync = async () => response = await authorizer.AuthorizeAsync2(command);

            context["given the validation pipeline fails"] = () =>
            {
                before = () =>
                    validationResult = new ValidationResult { IsValid = false, ErrorCode = "test_invalid" };

                it["returns the validation error code"] = () =>
                    response.AsT2.Errors.ShouldContain("test_invalid");
            };

            context["given the merchant is invalid"] = () =>
            {
                // before = () => merchantValidator
                //     .Setup(x => x.IsValid(command.MerchantId))
                //     .ReturnsAsync(false);
                before = () => command.MerchantId = -1;

                it["returns 'merchant_invalid' validation error"] = () =>
                    response.AsT2.Errors.ShouldContain("merchant_invalid");
            };

            context["given the currency is invalid"] = () =>
            {
                before = () =>
                {
                    currencyValidator
                        .Setup(x => x.IsValid(command.Currency))
                        .ReturnsAsync(false);
                };

                it["returns 'currency_invalid' validation error"] = () =>
                    response.AsT2.Errors.ShouldContain("currency_invalid");

                // TODO test if currency is valid etc.
            };

            context["given the merchant is a financial institution"] = () =>
            {
                before = () =>
                {
                    finValidator
                        .Setup(x => x.IsValid(command.MerchantId, command.RecipientName))
                        .ReturnsAsync(false);

                    command.RecipientName = null;
                };

                context["and the recipient is not valid"] = () =>
                {
                    it["returns 'recipient_required' validation error"] = () =>
                        response.AsT2.Errors.ShouldContain("recipient_required");

                };
            };
        }
    }
}