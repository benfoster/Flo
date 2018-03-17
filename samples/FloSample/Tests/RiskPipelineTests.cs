using System.Threading.Tasks;
using NSpec;
using Shouldly;

namespace FloSample.Tests
{
    class describe_risk_pipeline : nspec
    {
        async Task it_checks_risk()
        {
            var pipeline = RiskPipeline.Build();

            var context = new RiskContext
            {
                Amount = 2000,
                CustomerCountry = "GB"
            };

            context = await pipeline.Invoke(context);

            context.Result.Passed.ShouldBe(false);
            context.Result.RiskChecks.ShouldContainKeyAndValue("amount_max", false);
            context.Result.RiskChecks.ShouldContainKeyAndValue("customer_country", true);
            context.Result.Requires3ds.ShouldBe(true);
        }
    }
}