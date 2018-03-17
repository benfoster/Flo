using System;
using System.Threading;
using System.Threading.Tasks;
using Flo;

namespace FloSample
{
    public class CustomerCountryCheck : IHandler<RiskContext>
    {
        public Task<RiskContext> HandleAsync(RiskContext riskContext, Func<RiskContext, Task<RiskContext>> next)
        {
            bool passed = true;

            if (riskContext.CustomerCountry == "US")
                passed = false;

            if (riskContext.CustomerCountry == "GB")
                riskContext.Result.Requires3ds = true;

            riskContext.Result.RiskChecks.Add("customer_country", passed);

            return next.Invoke(riskContext);
        }
    }
}