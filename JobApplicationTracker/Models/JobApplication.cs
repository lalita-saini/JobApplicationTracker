using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplicationTracker.Models
{
    public class JobApplication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Company name must be between 2 and 100 characters")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Position must be between 2 and 100 characters")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [EnumDataType(typeof(ApplicationStatus), ErrorMessage = "Invalid application status")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Date applied is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        [ValidDateRange(ErrorMessage = "Date applied cannot be in the future")]
        public DateTime DateApplied { get; set; }

        [Required]
        public int UserId { get; set; }  // Foreign key to User
    }

    public enum ApplicationStatus
    {
        Applied,
        Interview,
        Offer,
        Rejected,
    }

    public class ValidDateRangeAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime date = (DateTime)value;
            return date <= DateTime.UtcNow;
        }
    }
}
