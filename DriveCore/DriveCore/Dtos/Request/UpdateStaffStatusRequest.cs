using System.ComponentModel.DataAnnotations;

namespace DriveCore.Dtos.Request
{
    public class UpdateStaffStatusRequest
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
