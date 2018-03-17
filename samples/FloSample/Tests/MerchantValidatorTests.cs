using System.Threading;
using System.Threading.Tasks;
using NSpec;
using Shouldly;

namespace FloSample
{
    class describe_merchant_validator : nspec
    {
        MerchantValidator validator;
        RequestPayment command;
        ValidationResult validationResult;

        void before_each()
        {
            validator = new MerchantValidator();


            command = new RequestPayment();
        }

        async Task act_each()
        {
            validationResult = await validator.HandleAsync(
                command,
                x => Task.FromResult(validationResult),
                CancellationToken.None);
        }

        void given_merchant_id_is_less_than_0()
        {
            before = () => command.MerchantId = -1;

            it["is not valid"] = () => validationResult.IsValid.ShouldBe(false);
            it["returns error 'merchant_invalid'"] = () => validationResult.ErrorCode.ShouldBe("merchant_invalid");
        }

        void given_merchant_id_is_greater_than_0()
        {
            before = () =>
            {
                command.MerchantId = 100;
                validationResult.IsValid = false;
                validationResult.ErrorCode = "next";
            };

            it["calls the next handler"] = () => validationResult.ErrorCode.ShouldBe("next");
        }
    }
}