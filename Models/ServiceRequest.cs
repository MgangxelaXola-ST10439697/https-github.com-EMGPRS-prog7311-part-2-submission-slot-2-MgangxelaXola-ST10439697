using System.ComponentModel.DataAnnotations;

namespace Global_Logistics_Managemant_System_POE.Models
{
    public class ServiceRequest
    {
        [Key]
        public int ServiceRequestId { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        [Display(Name = "Cost (USD)")]
        public double CostUSD { get; set; }

        [Display(Name = "Cost (ZAR)")]
        public double CostZAR { get; set; }

        // Foreign key
        public int ContractId { get; set; }
        public Contract? Contract { get; set; }
    }
}