using System.ComponentModel.DataAnnotations;

namespace Global_Logistics_Managemant_System_POE.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Client name is required.")]
        [Display(Name = "Client Name")]
        public string ClientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact details are required.")]
        [Display(Name = "Contact Details")]
        public string ContactDetails { get; set; } = string.Empty;

        [Required(ErrorMessage = "Region is required.")]
        public string Region { get; set; } = string.Empty;

        // Navigation properties 
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}