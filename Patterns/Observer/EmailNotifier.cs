using Global_Logistics_Managemant_System_POE.Models;

namespace Global_Logistics_Managemant_System_POE.Patterns.Observer
{
    public class EmailNotifier : IObserver
    {
        public void Update(Contract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract), "Contract cannot be null.");
            }
            try
            {
                // Simulate sending an email notification
                Console.WriteLine($"Email Notification: Contract {contract.ContractId} status has been updated to {contract.Status}.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error sending email notification.", ex);
            }
        }
    }
}
