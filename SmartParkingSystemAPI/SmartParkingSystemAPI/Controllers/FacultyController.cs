using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParkingSystemAPI.Data;
using SmartParkingSystemAPI.Models;
using SmartParkingSystemAPI.Dtos;


namespace SmartParkingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FacultyController : ControllerBase
    {
        private readonly ParkingDbContext _context;
        private readonly ILogger<FacultyController> _logger;

        public FacultyController(ParkingDbContext context, ILogger<FacultyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>GET /api/faculty — Retrieve all faculties (without spaces).</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacultyDto>>> GetAll()
        {
            var faculties = await _context.Faculties
                .AsNoTracking()
                .OrderBy(f => f.Name)
                .Select(f => MapToDto(f, new List<ParkingSpace>()))
                .ToListAsync();

            return Ok(faculties);
        }

        /// <summary>GET /api/faculty/{id} — Retrieve a faculty with its parking spaces.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FacultyDto>> GetById(int id)
        {
            var faculty = await _context.Faculties
                .AsNoTracking()
                .Include(f => f.ParkingSpaces)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (faculty is null)
                return NotFound(new { message = $"Faculty with id {id} not found." });

            return Ok(MapToFacultyDto(faculty));
        }

        /// <summary>GET /api/faculty/{id}/spaces — Retrieve parking spaces for a faculty.</summary>
        [HttpGet("{id:int}/spaces")]
        public async Task<ActionResult<IEnumerable<ParkingSpaceDto>>> GetSpaces(int id)
        {
            var exists = await _context.Faculties.AnyAsync(f => f.Id == id);
            if (!exists)
                return NotFound(new { message = $"Faculty with id {id} not found." });

            var spaces = await _context.ParkingSpaces
                .AsNoTracking()
                .Include(s => s.Faculty)
                .Where(s => s.FacultyId == id)
                .OrderBy(s => s.SpaceNumber)
                .Select(s => MapSpaceToDto(s))
                .ToListAsync();

            return Ok(spaces);
        }

      

        private static FacultyDto MapToFacultyDto(Faculty f) => new()
        {
            Id = f.Id,
            Name = f.Name,
            Latitude = f.Latitude,
            Longitude = f.Longitude,
            Description = f.Description,
            ParkingMonitoringEnabled = f.ParkingMonitoringEnabled,
            ParkingSpaces = f.ParkingSpaces.Select(MapSpaceToDto).ToList()
        };

        private static FacultyDto MapToDto(Faculty f, List<ParkingSpace> spaces) => new()
        {
            Id = f.Id,
            Name = f.Name,
            Latitude = f.Latitude,
            Longitude = f.Longitude,
            Description = f.Description,
            ParkingMonitoringEnabled = f.ParkingMonitoringEnabled,
            ParkingSpaces = spaces.Select(MapSpaceToDto).ToList()
        };

        private static ParkingSpaceDto MapSpaceToDto(ParkingSpace s) => new()
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