using System.Collections.Generic;
using System.Linq;

namespace FloSample
{
    public class RiskAssessment
    {
        public RiskAssessment()
        {
            RiskChecks = new Dictionary<string, bool>();
        }

        public Dictionary<string, bool> RiskChecks { get; set; }
        public bool Requires3ds { get; set; }
        public bool Passed => !RiskChecks.Any(c => !c.Value);
    }
}