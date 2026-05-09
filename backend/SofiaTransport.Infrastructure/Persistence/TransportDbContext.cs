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
    public DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();

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

        modelBuilder.Entity<UserFavorite>(e =>
        {
            e.ToTable("user_favorites");
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).HasColumnName("id").ValueGeneratedOnAdd();
            e.Property(f => f.UserId).HasColumnName("user_id").IsRequired();
            e.Property(f => f.EntityType).HasColumnName("entity_type").IsRequired().HasMaxLength(20);
            e.Property(f => f.EntityId).HasColumnName("entity_id").IsRequired().HasMaxLength(50);
            e.Property(f => f.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            e.HasIndex(f => new { f.UserId, f.EntityType, f.EntityId }).IsUnique().HasDatabaseName("idx_user_favorites_unique");
            e.HasIndex(f => f.UserId).HasDatabaseName("idx_user_favorites_user");
        });
    }
}
