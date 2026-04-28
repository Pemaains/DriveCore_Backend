using System.ComponentModel.DataAnnotations;

namespace DriveCore.Dtos.Request
{
    public class CreateCustomerRequest
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;

        [MinLength(1, ErrorMessage = "At least one vehicle is required.")]
        public List<CreateVehicleRequest> Vehicles { get; set; } = new();
    }
}
