namespace Global_Logistics_Managemant_System_POE.Patterns.Decorator
{
    public class ServiceRequestBase : IServiceRequest
    {
        // base cost of service request in USD
        public double Cost { get; set; }

        //returns the base cost with on additional logic
        public double CalculateCost()
        {
            return Cost;
        }
    }
}
