using System;
using System.Threading;
using System.Threading.Tasks;
using Flo;

namespace FloSample
{
    public class AmountRiskCheck : IHandler<RiskContext>
    {
        public Task<RiskContext> HandleAsync(RiskContext riskContext, Func<RiskContext, Task<RiskContext>> next)
        {
            bool passed = true;
            if (riskContext.Amount > 1000)
            {
                passed = false;
            };

            riskContext.Result.RiskChecks.Add("amount_max", passed);
            return next.Invoke(riskContext);
        }
    }
}