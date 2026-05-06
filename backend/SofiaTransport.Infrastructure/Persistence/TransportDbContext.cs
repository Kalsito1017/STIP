using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using SofiaTransport.Domain.Entities;
using SofiaTransport.Domain.Enums;
using SofiaTransport.Domain.ValueObjects;

namespace SofiaTransport.Infrastructure.Persistence;

public class TransportDbContext : DbContext
{
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Stop> Stops => Set<Stop>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<StopTime> StopTimes => Set<StopTime>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<DelayLog> DelayLogs => Set<DelayLog>();
    public DbSet<ReliabilityScore> ReliabilityScores => Set<ReliabilityScore>();
    public DbSet<Shape> Shapes => Set<Shape>();
    public DbSet<User> Users => Set<User>();

    public TransportDbContext(DbContextOptions<TransportDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Route>(e =>
        {
            e.ToTable("routes");
            e.HasKey(r => r.RouteId);
            e.Property(r => r.RouteId).HasColumnName("route_id");
            e.Property(r => r.ShortName).HasColumnName("short_name").IsRequired();
            e.Property(r => r.LongName).HasColumnName("long_name");
            e.Property(r => r.Type).HasColumnName("route_type");

            e.HasData(
                new Route { RouteId = "r-204", ShortName = "204", LongName = "Gotse Delchev – Orlov Most", Type = TransitType.Bus },
                new Route { RouteId = "r-94", ShortName = "94", LongName = "Studentski Grad – Sofia University", Type = TransitType.Bus },
                new Route { RouteId = "r-285", ShortName = "285", LongName = "Mladost 1 – Lyulin", Type = TransitType.Bus },
                new Route { RouteId = "r-260", ShortName = "260", LongName = "Druzhba – Central Station", Type = TransitType.Bus },
                new Route { RouteId = "r-72", ShortName = "72", LongName = "Zaharna Fabrika – Poduene", Type = TransitType.Bus },
                new Route { RouteId = "r-tram-1", ShortName = "1", LongName = "Sofia University – Mladost 1", Type = TransitType.Tram },
                new Route { RouteId = "r-tram-7", ShortName = "7", LongName = "Borovo – Lyulin", Type = TransitType.Tram },
                new Route { RouteId = "r-tram-10", ShortName = "10", LongName = "Vitosha – Zapaden Park", Type = TransitType.Tram },
                new Route { RouteId = "r-trol-1", ShortName = "1", LongName = "Stochna Gara – Ivan Vazov", Type = TransitType.Trolley },
                new Route { RouteId = "r-trol-2", ShortName = "2", LongName = "Hadzhi Dimitar – Buxton", Type = TransitType.Trolley },
                new Route { RouteId = "r-trol-5", ShortName = "5", LongName = "Mladost 2 – NDK", Type = TransitType.Trolley },
                new Route { RouteId = "r-trol-9", ShortName = "9", LongName = "Iztok – Gotse Delchev", Type = TransitType.Trolley },
                new Route { RouteId = "r-m1", ShortName = "M1", LongName = "Slivnitsa – Business Park", Type = TransitType.Metro },
                new Route { RouteId = "r-m2", ShortName = "M2", LongName = "Obelya – Vitosha", Type = TransitType.Metro },
                new Route { RouteId = "r-m3", ShortName = "M3", LongName = "Hadzhi Dimitar – Krasno Selo", Type = TransitType.Metro },
                new Route { RouteId = "r-m4", ShortName = "M4", LongName = "Obelya – Sofia Airport", Type = TransitType.Metro }
            );
        });

        modelBuilder.Entity<Stop>(e =>
        {
            e.ToTable("stops");
            e.HasKey(s => s.StopId);
            e.Property(s => s.StopId).HasColumnName("stop_id");
            e.Property(s => s.StopName).HasColumnName("stop_name").IsRequired();
            e.Ignore(s => s.Location);
            e.Property(s => s.Geometry)
                .HasColumnName("location")
                .HasColumnType("geography(POINT, 4326)");
            e.HasIndex(s => s.StopName).HasDatabaseName("idx_stops_name");
            e.HasIndex(s => s.Geometry).HasMethod("GIST").HasDatabaseName("idx_stops_location");

            e.HasData(
                new { StopId = "s-001", StopName = "Orlov Most", Geometry = new Point(23.3342, 42.6897) { SRID = 4326 } },
                new { StopId = "s-002", StopName = "Sofia University", Geometry = new Point(23.3451, 42.6939) { SRID = 4326 } },
                new { StopId = "s-003", StopName = "NDK", Geometry = new Point(23.3186, 42.6871) { SRID = 4326 } },
                new { StopId = "s-004", StopName = "Serdika", Geometry = new Point(23.3219, 42.6977) { SRID = 4326 } },
                new { StopId = "s-005", StopName = "Central Station", Geometry = new Point(23.3216, 42.7104) { SRID = 4326 } },
                new { StopId = "s-006", StopName = "Mladost 1", Geometry = new Point(23.3782, 42.6564) { SRID = 4326 } },
                new { StopId = "s-007", StopName = "Vitosha", Geometry = new Point(23.3140, 42.6630) { SRID = 4326 } },
                new { StopId = "s-008", StopName = "Obelya", Geometry = new Point(23.2600, 42.7460) { SRID = 4326 } },
                new { StopId = "s-009", StopName = "Lyulin", Geometry = new Point(23.2640, 42.7190) { SRID = 4326 } },
                new { StopId = "s-010", StopName = "Druzhba", Geometry = new Point(23.3950, 42.6620) { SRID = 4326 } },
                new { StopId = "s-011", StopName = "Iztok", Geometry = new Point(23.3520, 42.6870) { SRID = 4326 } },
                new { StopId = "s-012", StopName = "Lozenets", Geometry = new Point(23.3230, 42.6780) { SRID = 4326 } },
                new { StopId = "s-013", StopName = "Poduene", Geometry = new Point(23.3460, 42.7060) { SRID = 4326 } },
                new { StopId = "s-014", StopName = "Krasno Selo", Geometry = new Point(23.3000, 42.6800) { SRID = 4326 } },
                new { StopId = "s-015", StopName = "Borovo", Geometry = new Point(23.2920, 42.6680) { SRID = 4326 } },
                new { StopId = "s-016", StopName = "Studentski Grad", Geometry = new Point(23.3450, 42.6530) { SRID = 4326 } },
                new { StopId = "s-017", StopName = "Gotse Delchev", Geometry = new Point(23.2880, 42.6650) { SRID = 4326 } },
                new { StopId = "s-018", StopName = "Zaharna Fabrika", Geometry = new Point(23.2950, 42.7200) { SRID = 4326 } }
            );
        });

        modelBuilder.Entity<Trip>(e =>
        {
            e.ToTable("trips");
            e.HasKey(t => t.TripId);
            e.Property(t => t.TripId).HasColumnName("trip_id");
            e.Property(t => t.RouteId).HasColumnName("route_id");
            e.Property(t => t.ServiceId).HasColumnName("service_id");
            e.Property(t => t.DirectionId).HasColumnName("direction_id");
            e.HasOne(t => t.Route).WithMany(r => r.Trips).HasForeignKey(t => t.RouteId);
        });

        modelBuilder.Entity<StopTime>(e =>
        {
            e.ToTable("stop_times");
            e.HasKey(st => new { st.TripId, st.StopSequence });
            e.Property(st => st.TripId).HasColumnName("trip_id");
            e.Property(st => st.StopId).HasColumnName("stop_id");
            e.Property(st => st.StopSequence).HasColumnName("stop_sequence");
            e.Property(st => st.ArrivalTime).HasColumnName("arrival_time");
            e.Property(st => st.DepartureTime).HasColumnName("departure_time");
            e.HasOne(st => st.Trip).WithMany(t => t.StopTimes).HasForeignKey(st => st.TripId);
            e.HasOne(st => st.Stop).WithMany().HasForeignKey(st => st.StopId);
            e.HasIndex(st => new { st.StopId, st.ArrivalTime }).HasDatabaseName("idx_stop_times_stop_arrival");
            e.HasIndex(st => new { st.TripId, st.StopId }).HasDatabaseName("idx_stop_times_trip_stop");
        });

        modelBuilder.Entity<Vehicle>(e =>
        {
            e.ToTable("vehicles");
            e.HasKey(v => v.VehicleId);
            e.Property(v => v.VehicleId).HasColumnName("vehicle_id");
            e.Property(v => v.RouteId).HasColumnName("route_id");
            e.Property(v => v.TripId).HasColumnName("trip_id");
            e.Property(v => v.Bearing).HasColumnName("bearing");
            e.Property(v => v.Speed).HasColumnName("speed");
            e.Property(v => v.RecordedAt).HasColumnName("recorded_at").HasDefaultValueSql("now()");
            e.Ignore(v => v.Location);
            e.Property(v => v.Geometry)
                .HasColumnName("location")
                .HasColumnType("geography(POINT, 4326)");
            e.HasIndex(v => v.RecordedAt).HasDatabaseName("idx_vehicles_recorded_at").IsDescending();
            e.HasIndex(v => new { v.RouteId, v.RecordedAt }).HasDatabaseName("idx_vehicles_route_id").IsDescending(false, true);
            e.HasIndex(v => v.TripId).HasDatabaseName("idx_vehicles_trip_id");
            e.HasIndex(v => v.Geometry).HasMethod("GIST").HasDatabaseName("idx_vehicles_location");
        });

        modelBuilder.Entity<DelayLog>(e =>
        {
            e.ToTable("delay_logs");
            e.HasKey(d => d.Id);
            e.Property(d => d.Id).HasColumnName("id");
            e.Property(d => d.VehicleId).HasColumnName("vehicle_id");
            e.Property(d => d.StopId).HasColumnName("stop_id");
            e.Property(d => d.TripId).HasColumnName("trip_id");
            e.Property(d => d.RouteId).HasColumnName("route_id");
            e.Property(d => d.ScheduledArrival).HasColumnName("scheduled_arrival");
            e.Property(d => d.ActualArrival).HasColumnName("actual_arrival");
            e.Property(d => d.DelaySeconds).HasColumnName("delay_seconds");
            e.Property(d => d.RecordedAt).HasColumnName("recorded_at").HasDefaultValueSql("now()");
            e.HasIndex(d => new { d.RouteId, d.RecordedAt }).HasDatabaseName("idx_delay_logs_route").IsDescending(false, true);
            e.HasIndex(d => new { d.StopId, d.RecordedAt }).HasDatabaseName("idx_delay_logs_stop").IsDescending(false, true);
            e.HasIndex(d => d.RecordedAt).HasDatabaseName("idx_delay_logs_recorded_at").IsDescending();
            e.HasIndex(d => new { d.VehicleId, d.RecordedAt }).HasDatabaseName("idx_delay_logs_vehicle").IsDescending(false, true);
        });

        modelBuilder.Entity<ReliabilityScore>(e =>
        {
            e.ToTable("reliability_scores");
            e.HasKey(r => new { r.RouteId, r.ScoreDate });
            e.Property(r => r.RouteId).HasColumnName("route_id");
            e.Property(r => r.ScoreDate).HasColumnName("score_date");
            e.Property(r => r.OnTimePct).HasColumnName("on_time_pct");
            e.Property(r => r.AvgDelaySeconds).HasColumnName("avg_delay_seconds");
            e.Property(r => r.Score).HasColumnName("reliability_score");
            e.Property(r => r.PeakScore).HasColumnName("peak_score");
            e.Property(r => r.SampleCount).HasColumnName("sample_count").HasDefaultValue(0);
        });

        modelBuilder.Entity<Shape>(e =>
        {
            e.ToTable("shapes");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();
            e.Property(s => s.RouteId).HasColumnName("route_id").IsRequired();
            e.Property(s => s.Sequence).HasColumnName("sequence");
            e.Property(s => s.Lat).HasColumnName("lat");
            e.Property(s => s.Lon).HasColumnName("lon");
            e.HasOne(s => s.Route).WithMany(r => r.Shapes).HasForeignKey(s => s.RouteId);
            e.HasIndex(s => new { s.RouteId, s.Sequence }).HasDatabaseName("idx_shapes_route_sequence").IsUnique();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Email).HasColumnName("email").IsRequired();
            e.HasIndex(u => u.Email).IsUnique().HasDatabaseName("idx_users_email");
            e.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
            e.Property(u => u.FullName).HasColumnName("full_name").IsRequired();
            e.Property(u => u.CreatedAt).HasColumnName("created_at");
        });
    }
}
