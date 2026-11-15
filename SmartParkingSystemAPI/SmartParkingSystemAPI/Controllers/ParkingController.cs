using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartParkingSystemAPI.Data;
using SmartParkingSystemAPI.Hubs;
using SmartParkingSystemAPI.Models;
using SmartParkingSystemAPI.Dtos;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SmartParkingSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ParkingController : ControllerBase
    {
        private readonly ParkingDbContext _context;
        private readonly IHubContext<ParkingHub> _hubContext;
        private readonly ILogger<ParkingController> _logger;

        public ParkingController(
            ParkingDbContext context,
            IHubContext<ParkingHub> hubContext,
            ILogger<ParkingController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>GET /api/parking — Retrieve all parking spaces.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParkingSpaceDto>>> GetAll()
        {
            var spaces = await _context.ParkingSpaces
                .AsNoTracking()
                .Include(s => s.Faculty)
                .OrderBy(s => s.SpaceNumber)
                .Select(s => MapToDto(s))
                .ToListAsync();

            return Ok(spaces);
        }

        /// <summary>GET /api/parking/{spaceNumber} — Retrieve a specific parking space.</summary>
        [HttpGet("{spaceNumber}")]
        public async Task<ActionResult<ParkingSpaceDto>> GetBySpaceNumber(string spaceNumber)
        {
            var space = await _context.ParkingSpaces
                .AsNoTracking()
                .Include(s => s.Faculty)
                .FirstOrDefaultAsync(s => s.SpaceNumber == spaceNumber);

            if (space is null)
                return NotFound(new { message = $"Parking space '{spaceNumber}' not found." });

            return Ok(MapToDto(space));
        }

        /// <summary>
        /// POST /api/parking/update — Update parking status from ESP32.
        /// Broadcasts real-time updates via SignalR when status changes.
        /// </summary>
        [HttpPost("update")]
        public async Task<ActionResult<ParkingSpaceDto>> UpdateStatus([FromBody] ParkingUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.SpaceNumber))
                return BadRequest(new { message = "SpaceNumber is required." });

            if (!Enum.IsDefined(typeof(ParkingStatus), dto.Status))
                return BadRequest(new { message = $"Invalid status value: {dto.Status}." });

            var space = await _context.ParkingSpaces
                .Include(s => s.Faculty)
                .FirstOrDefaultAsync(s => s.SpaceNumber == dto.SpaceNumber);

            if (space is null)
                return NotFound(new { message = $"Parking space '{dto.SpaceNumber}' not found." });

            var newStatus = (ParkingStatus)dto.Status;
            bool statusChanged = space.Status != newStatus;

            space.Status = newStatus;
            space.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Broadcast via SignalR only when status actually changes
            if (statusChanged)
            {
                var update = new SignalRUpdateDto
                {
                    SpaceId = space.Id,
                    SpaceNumber = space.SpaceNumber,
                    FacultyId = space.FacultyId,
                    Status = space.Status.ToString(),
                    LastUpdated = space.LastUpdated
                };

                await _hubContext.Clients.Group("ParkingMonitors")
                    .SendAsync("ReceiveParkingUpdate", update);

                _logger.LogInformation(
                    "Space {SpaceNumber} updated to {Status} and broadcast via SignalR.",
                    space.SpaceNumber, space.Status);
            }

            return Ok(MapToDto(space));
        }

        /// <summary>GET /api/parking/stats — Retrieve system-wide parking statistics.</summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ParkingStatsDto>> GetStats()
        {
            var faculties = await _context.Faculties.AsNoTracking().ToListAsync();
            var spaces = await _context.ParkingSpaces.AsNoTracking().ToListAsync();

            var stats = new ParkingStatsDto
            {
                TotalFaculties = faculties.Count,
                ActiveFaculties = faculties.Count(f => f.ParkingMonitoringEnabled),
                ComingSoonFaculties = faculties.Count(f => !f.ParkingMonitoringEnabled),
                TotalSpaces = spaces.Count,
                AvailableSpaces = spaces.Count(s => s.Status == ParkingStatus.Available),
                OccupiedSpaces = spaces.Count(s => s.Status == ParkingStatus.Occupied),
                InactiveSpaces = spaces.Count(s => s.Status == ParkingStatus.Inactive)
            };

            return Ok(stats);
        }

        /// <summary>GET /api/parking/all — Parking spaces grouped by faculty.</summary>
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<FacultyGroupDto>>> GetAllGrouped()
        {
            var faculties = await _context.Faculties
                .AsNoTracking()
                .Include(f => f.ParkingSpaces)
                .OrderBy(f => f.Name)
                .ToListAsync();

            var grouped = faculties.Select(f => new FacultyGroupDto
            {
                Faculty = f.Name,
                FacultyId = f.Id,
                MonitoringEnabled = f.ParkingMonitoringEnabled,
                Spaces = f.ParkingSpaces.Select(s => new ParkingSpaceDto
                {
                    Id = s.Id,
                    SpaceNumber = s.SpaceNumber,
                    Status = s.Status.ToString(),
                    StatusCode = (int)s.Status,
                    LastUpdated = s.LastUpdated,
                    HasSensor = s.HasSensor,
                    SensorId = s.SensorId,
                    FacultyId = s.FacultyId,
                    FacultyName = f.Name
                }).OrderBy(s => s.SpaceNumber).ToList()
            }).ToList();

            return Ok(grouped);
        }

        // ── Mapper ────────────────────────────────────────────────────────────────

        private static ParkingSpaceDto MapToDto(ParkingSpace s) => new()
        {
            Id = s.Id,
            SpaceNumber = s.SpaceNumber,
            Status = s.Status.ToString(),
            StatusCode = (int)s.Status,
            LastUpdated = s.LastUpdated,
            HasSensor = s.HasSensor,
            SensorId = s.SensorId,
            FacultyId = s.FacultyId,
            FacultyName = s.Faculty?.Name ?? string.Empty
        };
    }
}
