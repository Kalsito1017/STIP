using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SofiaTransport.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndicesAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_stop_times_stop_id",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "IX_delay_logs_route_id_recorded_at",
                table: "delay_logs");

            migrationBuilder.DropIndex(
                name: "IX_delay_logs_stop_id_recorded_at",
                table: "delay_logs");

            migrationBuilder.AlterColumn<int>(
                name: "delay_seconds",
                table: "delay_logs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.InsertData(
                table: "routes",
                columns: new[] { "route_id", "long_name", "short_name", "route_type" },
                values: new object[,]
                {
                    { "r-204", "Gotse Delchev – Orlov Most", "204", 3 },
                    { "r-260", "Druzhba – Central Station", "260", 3 },
                    { "r-285", "Mladost 1 – Lyulin", "285", 3 },
                    { "r-72", "Zaharna Fabrika – Poduene", "72", 3 },
                    { "r-94", "Studentski Grad – Sofia University", "94", 3 },
                    { "r-m1", "Slivnitsa – Business Park", "M1", 1 },
                    { "r-m2", "Obelya – Vitosha", "M2", 1 },
                    { "r-m3", "Hadzhi Dimitar – Krasno Selo", "M3", 1 },
                    { "r-m4", "Obelya – Sofia Airport", "M4", 1 },
                    { "r-tram-1", "Ivan Vazov – Nadezhda", "1", 0 },
                    { "r-tram-10", "Vitosha – Zapaden Park", "10", 0 },
                    { "r-tram-7", "Borovo – Lyulin", "7", 0 },
                    { "r-trol-1", "Stochna Gara – Ivan Vazov", "1", 11 },
                    { "r-trol-2", "Hadzhi Dimitar – Buxton", "2", 11 },
                    { "r-trol-5", "Mladost 2 – NDK", "5", 11 },
                    { "r-trol-9", "Iztok – Gotse Delchev", "9", 11 }
                });

            migrationBuilder.InsertData(
                table: "stops",
                columns: new[] { "stop_id", "location", "stop_name" },
                values: new object[,]
                {
                    { "s-001", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.3342 42.6897)"), "Orlov Most" },
                    { "s-002", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.3451 42.6939)"), "Sofia University" },
                    { "s-003", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.3186 42.6871)"), "NDK" },
                    { "s-004", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.3219 42.6977)"), "Serdika" },
                    { "s-005", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.3216 42.7104)"), "Central Station" },
                    { "s-006", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.3782 42.6564)"), "Mladost 1" },
                    { "s-007", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.314 42.663)"), "Vitosha" },
                    { "s-008", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.26 42.746)"), "Obelya" },
                    { "s-009", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.264 42.719)"), "Lyulin" },
                    { "s-010", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.395 42.662)"), "Druzhba" },
                    { "s-011", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.352 42.687)"), "Iztok" },
                    { "s-012", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.323 42.678)"), "Lozenets" },
                    { "s-013", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.346 42.706)"), "Poduene" },
                    { "s-014", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.3 42.68)"), "Krasno Selo" },
                    { "s-015", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.292 42.668)"), "Borovo" },
                    { "s-016", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.345 42.653)"), "Studentski Grad" },
                    { "s-017", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.288 42.665)"), "Gotse Delchev" },
                    { "s-018", (NetTopologySuite.Geometries.Point)new NetTopologySuite.IO.WKTReader().Read("SRID=4326;POINT (23.295 42.72)"), "Zaharna Fabrika" }
                });

            migrationBuilder.CreateIndex(
                name: "idx_vehicles_recorded_at",
                table: "vehicles",
                column: "recorded_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_vehicles_route_id",
                table: "vehicles",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "idx_stops_name",
                table: "stops",
                column: "stop_name");

            migrationBuilder.CreateIndex(
                name: "idx_stop_times_stop_arrival",
                table: "stop_times",
                columns: new[] { "stop_id", "arrival_time" });

            migrationBuilder.CreateIndex(
                name: "idx_delay_logs_recorded_at",
                table: "delay_logs",
                column: "recorded_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_delay_logs_route",
                table: "delay_logs",
                columns: new[] { "route_id", "recorded_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_delay_logs_stop",
                table: "delay_logs",
                columns: new[] { "stop_id", "recorded_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "idx_delay_logs_vehicle",
                table: "delay_logs",
                columns: new[] { "vehicle_id", "recorded_at" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_vehicles_recorded_at",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "idx_vehicles_route_id",
                table: "vehicles");

            migrationBuilder.DropIndex(
                name: "idx_stops_name",
                table: "stops");

            migrationBuilder.DropIndex(
                name: "idx_stop_times_stop_arrival",
                table: "stop_times");

            migrationBuilder.DropIndex(
                name: "idx_delay_logs_recorded_at",
                table: "delay_logs");

            migrationBuilder.DropIndex(
                name: "idx_delay_logs_route",
                table: "delay_logs");

            migrationBuilder.DropIndex(
                name: "idx_delay_logs_stop",
                table: "delay_logs");

            migrationBuilder.DropIndex(
                name: "idx_delay_logs_vehicle",
                table: "delay_logs");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-204");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-260");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-285");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-72");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-94");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-m1");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-m2");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-m3");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-m4");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-tram-1");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-tram-10");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-tram-7");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-trol-1");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-trol-2");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-trol-5");

            migrationBuilder.DeleteData(
                table: "routes",
                keyColumn: "route_id",
                keyValue: "r-trol-9");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-001");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-002");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-003");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-004");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-005");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-006");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-007");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-008");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-009");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-010");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-011");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-012");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-013");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-014");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-015");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-016");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-017");

            migrationBuilder.DeleteData(
                table: "stops",
                keyColumn: "stop_id",
                keyValue: "s-018");

            migrationBuilder.AlterColumn<int>(
                name: "delay_seconds",
                table: "delay_logs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_stop_times_stop_id",
                table: "stop_times",
                column: "stop_id");

            migrationBuilder.CreateIndex(
                name: "IX_delay_logs_route_id_recorded_at",
                table: "delay_logs",
                columns: new[] { "route_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_delay_logs_stop_id_recorded_at",
                table: "delay_logs",
                columns: new[] { "stop_id", "recorded_at" });
        }
    }
}
