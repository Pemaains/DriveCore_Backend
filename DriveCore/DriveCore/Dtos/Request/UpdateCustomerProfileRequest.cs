using System.ComponentModel.DataAnnotations;

namespace DriveCore.Dtos.Request
{
    public class UpdateCustomerProfileRequest
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;
    }
}
