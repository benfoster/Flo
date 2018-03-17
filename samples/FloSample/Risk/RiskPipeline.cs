using System;
using System.Threading.Tasks;
using Flo;

namespace FloSample
{
    public class RiskPipeline
    {
        public static Func<RiskContext, Task<RiskContext>> Build()
        {
            return Pipeline.Build<RiskContext>(cfg =>
                cfg.Add<AmountRiskCheck>()
                .Add<CustomerCountryCheck>()
            );
        }
    }
}