using System.ComponentModel.DataAnnotations;

namespace DriveCore.Models
{
    public class CustomerProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        [Required]
        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;

        public string? CreatedByStaffId { get; set; }

        public ApplicationUser? CreatedByStaff { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
