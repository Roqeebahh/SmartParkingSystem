namespace SmartParkingSystemAPI.Dtos
{
    public class FacultyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool ParkingMonitoringEnabled { get; set; }
        public List<ParkingSpaceDto> ParkingSpaces { get; set; } = new();
    }

    public class ParkingSpaceDto
    {
        public int Id { get; set; }
        public string SpaceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool HasSensor { get; set; }
        public string? SensorId { get; set; }
        public int FacultyId { get; set; }
        public string FacultyName { get; set; } = string.Empty;
    }

    public class ParkingUpdateDto
    {
        public string SpaceNumber { get; set; } = string.Empty;
        public int Status { get; set; }
        public string? SensorId { get; set; }
        public double? Distance { get; set; }
    }

    public class ParkingStatsDto
    {
        public int TotalFaculties { get; set; }
        public int ActiveFaculties { get; set; }
        public int ComingSoonFaculties { get; set; }
        public int TotalSpaces { get; set; }
        public int AvailableSpaces { get; set; }
        public int OccupiedSpaces { get; set; }
        public int InactiveSpaces { get; set; }
    }

    public class FacultyGroupDto
    {
        public string Faculty { get; set; } = string.Empty;
        public int FacultyId { get; set; }
        public bool MonitoringEnabled { get; set; }
        public List<ParkingSpaceDto> Spaces { get; set; } = new();
    }

    public class SignalRUpdateDto
    {
        public int SpaceId { get; set; }
        public string SpaceNumber { get; set; } = string.Empty;
        public int FacultyId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }
}
