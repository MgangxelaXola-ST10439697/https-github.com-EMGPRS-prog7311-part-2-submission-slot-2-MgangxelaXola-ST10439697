using Global_Logistics_Managemant_System_POE.Models;

namespace Global_Logistics_Managemant_System_POE.Patterns.Observer
{
    //interface for observers that want to be notified of contract status changes
    public interface IObserver
    {
        void Update(Contract contract);
    }
}
