using System.ComponentModel.DataAnnotations;
using Global_Logistics_Managemant_System_POE.Patterns.Observer;

namespace Global_Logistics_Managemant_System_POE.Models
{
    public class Contract
    {
        [Key]
        public int ContractId { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Signed Agreement")]
        public string SignedAgreementPath { get; set; } = string.Empty;

        [Display(Name = "Service Level")]
        public string ServiceLevel { get; set; } = string.Empty;

        // Foreign key
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        //  Observer pattern 
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        private List<IObserver> _observers = new List<IObserver>();

        public void Attach(IObserver observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            _observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));
            _observers.Remove(observer);
        }

        public void Notify()
        {
            foreach (var observer in _observers)
                observer.Update(this);
        }

        public void UpdateStatus(string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Status cannot be empty.", nameof(newStatus));
            Status = newStatus;
            Notify();
        }
    }
}