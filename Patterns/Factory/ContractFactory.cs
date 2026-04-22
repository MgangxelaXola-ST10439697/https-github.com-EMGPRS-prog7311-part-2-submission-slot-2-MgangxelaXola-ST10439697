using Global_Logistics_Managemant_System_POE.Models;    

namespace Global_Logistics_Managemant_System_POE.Patterns.Factory
{
    public class ContractFactory
    {
        // valid statuses from the assignment 
        private static readonly string[] ValidStatuses = { "Draft", "Active", "Expired", "On Hold" };

        // factory method that decides which contract type to create
        public static Contract CreateContract(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Contract type cannot be empty.", nameof(type));
            }

            try
            {
                return type.ToLower() switch
                {
                    // SLA contract - linked to specific Service Level Agreements
                    "sla" => new SLAContract
                    {
                        Status = "Draft",
                        SlaTerms = "Standard SLA Terms Apply",
                        ResponseTime = "24 Hours",
                        PenaltyClause = "Standard penalty terms apply."
                    },

                    // standard contract - general logistics contract
                    "standard" => new Contract
                    {
                        Status = "Draft"
                    },

                    _ => throw new InvalidOperationException(
                        $"Contract type '{type}' is not supported. Valid types are: 'sla', 'standard'.")
                };
            }
            catch (InvalidOperationException)
            {
                // rethrow business rule violations as-is
                throw;
            }
            catch (Exception ex)
            {
                // wrap any unexpected errors
                throw new InvalidOperationException("Failed to create contract.", ex);
            }

        }

        // helper method to check if a status is valid
        public static bool IsValidStatus(string status)
        {
            return ValidStatuses.Contains(status);

        }
    }
}
