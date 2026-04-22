using Global_Logistics_Managemant_System_POE.Models;    

namespace Global_Logistics_Managemant_System_POE.Patterns.Factory
{
    
        public class SLAContract : Contract
        {
            public string SlaTerms { get; set; } = string.Empty;
            public string ResponseTime { get; set; } = string.Empty;
            public string PenaltyClause { get; set; } = string.Empty;
        }
    }

