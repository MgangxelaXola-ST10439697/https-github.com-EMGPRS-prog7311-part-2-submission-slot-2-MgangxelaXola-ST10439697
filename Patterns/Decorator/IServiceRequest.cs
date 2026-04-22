 namespace Global_Logistics_Managemant_System_POE.Patterns.Decorator
{
    //interface that all service request types will implement, allowing for consistent cost calculation
    public interface IServiceRequest
    {
        double CalculateCost();
    }
}
