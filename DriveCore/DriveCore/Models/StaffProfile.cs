using System.ComponentModel.DataAnnotations;

namespace DriveCore.Models
{
    public class StaffProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        public string StaffCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string Position { get; set; } = string.Empty;
    }
}
