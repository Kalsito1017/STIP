using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SofiaTransport.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "delay_logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vehicle_id = table.Column<string>(type: "text", nullable: true),
                    stop_id = table.Column<string>(type: "text", nullable: true),
                    trip_id = table.Column<string>(type: "text", nullable: true),
                    route_id = table.Column<string>(type: "text", nullable: true),
                    scheduled_arrival = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actual_arrival = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    delay_seconds = table.Column<int>(type: "integer", nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delay_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reliability_scores",
                columns: table => new
                {
                    route_id = table.Column<string>(type: "text", nullable: false),
                    score_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    on_time_pct = table.Column<double>(type: "double precision", nullable: false),
                    avg_delay_seconds = table.Column<double>(type: "double precision", nullable: false),
                    reliability_score = table.Column<double>(type: "double precision", nullable: false),
                    peak_score = table.Column<double>(type: "double precision", nullable: false),
                    sample_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reliability_scores", x => new { x.route_id, x.score_date });
                });

            migrationBuilder.CreateTable(
                name: "routes",
                columns: table => new
                {
                    route_id = table.Column<string>(type: "text", nullable: false),
                    short_name = table.Column<string>(type: "text", nullable: false),
                    long_name = table.Column<string>(type: "text", nullable: true),
                    route_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routes", x => x.route_id);
                });

            migrationBuilder.CreateTable(
                name: "stops",
                columns: table => new
                {
                    stop_id = table.Column<string>(type: "text", nullable: false),
                    stop_name = table.Column<string>(type: "text", nullable: false),
                    location = table.Column<Point>(type: "geography(POINT, 4326)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stops", x => x.stop_id);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    vehicle_id = table.Column<string>(type: "text", nullable: false),
                    route_id = table.Column<string>(type: "text", nullable: true),
                    trip_id = table.Column<string>(type: "text", nullable: true),
                    bearing = table.Column<float>(type: "real", nullable: false),
                    speed = table.Column<float>(type: "real", nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    location = table.Column<Point>(type: "geography(POINT, 4326)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.vehicle_id);
                });

            migrationBuilder.CreateTable(
                name: "trips",
                columns: table => new
                {
                    trip_id = table.Column<string>(type: "text", nullable: false),
                    route_id = table.Column<string>(type: "text", nullable: false),
                    service_id = table.Column<string>(type: "text", nullable: false),
                    direction_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trips", x => x.trip_id);
                    table.ForeignKey(
                        name: "FK_trips_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "routes",
                        principalColumn: "route_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stop_times",
                columns: table => new
                {
                    trip_id = table.Column<string>(type: "text", nullable: false),
                    stop_sequence = table.Column<int>(type: "integer", nullable: false),
                    stop_id = table.Column<string>(type: "text", nullable: false),
                    arrival_time = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stop_times", x => new { x.trip_id, x.stop_sequence });
                    table.ForeignKey(
                        name: "FK_stop_times_stops_stop_id",
                        column: x => x.stop_id,
                        principalTable: "stops",
                        principalColumn: "stop_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stop_times_trips_trip_id",
                        column: x => x.trip_id,
                        principalTable: "trips",
                        principalColumn: "trip_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_delay_logs_route_id_recorded_at",
                table: "delay_logs",
                columns: new[] { "route_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_delay_logs_stop_id_recorded_at",
                table: "delay_logs",
                columns: new[] { "stop_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_stop_times_stop_id",
                table: "stop_times",
                column: "stop_id");

            migrationBuilder.CreateIndex(
                name: "IX_trips_route_id",
                table: "trips",
                column: "route_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "delay_logs");

            migrationBuilder.DropTable(
                name: "reliability_scores");

            migrationBuilder.DropTable(
                name: "stop_times");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "stops");

            migrationBuilder.DropTable(
                name: "trips");

            migrationBuilder.DropTable(
                name: "routes");
        }
    }
}
