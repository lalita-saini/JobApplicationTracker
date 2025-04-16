using System.ComponentModel.DataAnnotations;

namespace JobApplicationTracker.Models.Dtos
{
    public class CreateJobApplicationDto
    {
        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [Required]
        [StringLength(100)]
        public string Position { get; set; }

        [Required]
        [RegularExpression("^(Applied|Interview|Offer|Rejected)$",
            ErrorMessage = "Status must be one of: Applied, Interview, Offer, Rejected")]
        public string Status { get; set; }

        [Required]
        public DateTime DateApplied { get; set; }
    }
}
