using DriveCore.Models;

namespace DriveCore.Dtos.Response
{
    public class StaffResponse
    {
        public string UserId { get; set; } = string.Empty;
        public int StaffProfileId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string StaffCode { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }
}
