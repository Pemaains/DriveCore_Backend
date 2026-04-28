using System.ComponentModel.DataAnnotations;

namespace DriveCore.Dtos.Request
{
    public class UpdateStaffRequest
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string Position { get; set; } = string.Empty;
    }
}
