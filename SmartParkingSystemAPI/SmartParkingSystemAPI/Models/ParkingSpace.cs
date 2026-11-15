using SmartParkingSystemAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartParkingSystemAPI.Models
{
    public enum ParkingStatus
    {
        Available = 0,
        Occupied = 1,
        Inactive = 2
    }

    public class ParkingSpace
    {
        public int Id { get; set; }
        public string SpaceNumber { get; set; } = string.Empty;
        public ParkingStatus Status { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool HasSensor { get; set; }
        public string? SensorId { get; set; }

        // Foreign Key
        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; } = null!;
    }
}
