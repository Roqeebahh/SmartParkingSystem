using Microsoft.EntityFrameworkCore;
using SmartParkingSystemAPI.Models;

namespace SmartParkingSystemAPI.Data
{
    public class ParkingDbContext : DbContext
    {
        public ParkingDbContext(DbContextOptions<ParkingDbContext> options)
        : base(options)
        {
        }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<ParkingSpace> ParkingSpaces { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Faculty>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            });


            modelBuilder.Entity<ParkingSpace>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SpaceNumber).HasMaxLength(20).IsRequired();
                entity.HasIndex(e => e.SpaceNumber).IsUnique();
                entity.Property(e => e.SensorId).HasMaxLength(50);
                entity.HasOne(e => e.Faculty)
                      .WithMany(f => f.ParkingSpaces)
                      .HasForeignKey(e => e.FacultyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });



            var faculties = new List<Faculty>
        {
            new() { Id = 1,  Name = "Faculty of Arts",                                  Latitude = 6.464215, Longitude = 3.201938, Description = "Humanities, languages and creative arts studies.", ParkingMonitoringEnabled = false },
            new() { Id = 2,  Name = "Faculty of Education",                             Latitude = 6.473019, Longitude = 3.199877, Description = "Teacher training and educational programmes.",        ParkingMonitoringEnabled = false },
            new() { Id = 3,  Name = "Faculty of Law",                                   Latitude = 6.467509, Longitude = 3.201960, Description = "Legal studies and jurisprudence.",                   ParkingMonitoringEnabled = false },
            new() { Id = 4,  Name = "Faculty of Management Sciences",                   Latitude = 6.476043, Longitude = 3.199862, Description = "Business, accounting and management studies.",       ParkingMonitoringEnabled = false },
            new() { Id = 5,  Name = "Faculty of Science",                               Latitude = 6.466292, Longitude = 3.200051, Description = "Pure and applied science programmes.",              ParkingMonitoringEnabled = false },
            new() { Id = 6,  Name = "Faculty of Social Sciences",                       Latitude = 6.475344, Longitude = 3.199207, Description = "Economics, sociology and political science.",        ParkingMonitoringEnabled = false },
            new() { Id = 7,  Name = "Faculty of Computing and Information Technology",  Latitude = 6.472964, Longitude = 3.199897, Description = "Computing, IT and software engineering hub.",        ParkingMonitoringEnabled = true  },
            new() { Id = 8,  Name = "Faculty of Environmental Sciences",                Latitude = 6.467294, Longitude = 3.200251, Description = "Architecture, urban planning and surveying.",        ParkingMonitoringEnabled = false },
            new() { Id = 9,  Name = "School of Communication",                          Latitude = 6.4682, Longitude = 3.1808, Description = "Mass communication and media studies.",              ParkingMonitoringEnabled = false },
            new() { Id = 10, Name = "School of Transport and Logistics",                Latitude = 6.474462, Longitude = 3.197995, Description = "Transportation management and logistics.",           ParkingMonitoringEnabled = false },
            new() { Id = 11, Name = "School of Library, Archival and Information Science", Latitude = 6.465000, Longitude = 3.200632, Description = "Library science and archival management.",       ParkingMonitoringEnabled = false },
            new() { Id = 12, Name = "School of Creativity, Culture and Tourism Studies",Latitude = 6.4643, Longitude = 3.1855, Description = "Creative arts, tourism and cultural studies.",       ParkingMonitoringEnabled = false },
            new() { Id = 13, Name = "Postgraduate School",                              Latitude = 6.468588, Longitude = 3.201234, Description = "Advanced degrees and research programmes.",          ParkingMonitoringEnabled = false },
        };

            modelBuilder.Entity<Faculty>().HasData(faculties);


            var spaces = new List<ParkingSpace>();
            int id = 1;
            var prefixes = new Dictionary<int, string>
        {
            {1,"ART"},{2,"EDU"},{3,"LAW"},{4,"MGT"},{5,"SCI"},
            {6,"SOC"},{7,"CIT"},{8,"ENV"},{9,"COM"},{10,"TRL"},
            {11,"LIB"},{12,"CCT"},{13,"PGS"}
        };

            var seedTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            foreach (var fac in faculties)
            {
                string prefix = prefixes[fac.Id];
                bool isCIT = fac.Id == 7;
                for (int i = 1; i <= 5; i++)
                {
                    spaces.Add(new ParkingSpace
                    {
                        Id = id++,
                        SpaceNumber = $"{prefix}-0{i}",
                        Status = isCIT ? ParkingStatus.Available : ParkingStatus.Inactive,
                        LastUpdated = seedTime,
                        HasSensor = isCIT,
                        SensorId = isCIT ? $"ESP32-CIT-0{i}" : null,
                        FacultyId = fac.Id
                    });
                }
            }

            modelBuilder.Entity<ParkingSpace>().HasData(spaces);
        }
    }
}

