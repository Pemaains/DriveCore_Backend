using System.ComponentModel.DataAnnotations;

namespace DriveCore.Dtos.Request
{
    public class UpdateVehicleRequest
    {
        [Required]
        [MaxLength(30)]
        public string VehicleNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)]
        public string Model { get; set; } = string.Empty;

        [Range(1900, 2100)]
        public int Year { get; set; }

        [MaxLength(40)]
        public string? Color { get; set; }
    }
}
