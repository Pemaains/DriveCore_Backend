using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DriveCore.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public StaffProfile? StaffProfile { get; set; }

        public CustomerProfile? CustomerProfile { get; set; }
    }
}
