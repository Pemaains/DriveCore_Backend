using DriveCore.Models;
using System.ComponentModel.DataAnnotations;

namespace DriveCore.Dtos.Request
{
    public class UpdateStaffRoleRequest
    {
        [Required]
        public UserRole Role { get; set; } = UserRole.Staff;
    }
}
