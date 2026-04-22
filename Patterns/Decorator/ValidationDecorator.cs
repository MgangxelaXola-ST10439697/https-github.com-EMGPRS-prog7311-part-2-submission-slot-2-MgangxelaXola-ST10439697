using Global_Logistics_Managemant_System_POE.Models;

namespace Global_Logistics_Managemant_System_POE.Patterns.Decorator
{
    public class ValidationDecorator : IServiceRequest
    {
      
        private readonly IServiceRequest _innerRequest;
        private readonly Contract _contract;

       
        public ValidationDecorator(IServiceRequest innerRequest, Contract contract)
        {
            _innerRequest = innerRequest;
            _contract = contract;
        }

        // wraps the base CalculateCost with contract status validation
        public double CalculateCost()
        {
            if (_contract == null)
            {
                throw new ArgumentNullException(nameof(_contract), "Contract information is required.");
            }

            try
            {
                // business rule: if contract is expired or on hold, service request cannot be processed
                if (_contract.Status == "Expired" || _contract.Status == "On Hold")
                {
                    throw new InvalidOperationException(
                        $"Cannot process service request. Contract is currently '{_contract.Status}'.");
                }

                // if valid, delegate to the inner request
                return _innerRequest.CalculateCost();
            }
            catch (InvalidOperationException)
            {
                // rethrow business rule violations as-is
                throw;
            }
            catch (Exception ex)
            {
                
                throw new InvalidOperationException("Service request validation failed.", ex);
            }

        }

    }

}
