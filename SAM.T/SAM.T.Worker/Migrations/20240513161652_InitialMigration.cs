using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SAM.T.Worker.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitoredApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Url = table.Column<string>(type: "text", nullable: false),
                    UseProxy = table.Column<bool>(type: "boolean", nullable: false),
                    ProxyUrl = table.Column<string>(type: "text", nullable: true),
                    ProxyPort = table.Column<int>(type: "integer", nullable: true),
                    ProxyUsername = table.Column<string>(type: "text", nullable: true),
                    ProxyPassword = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoredApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResponseCode = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Fail = table.Column<string>(type: "text", nullable: true),
                    ResponseTime = table.Column<long>(type: "bigint", nullable: false),
                    ResponseTimeDeviation = table.Column<double>(type: "double precision", nullable: false),
                    MonitoredApplicationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitoringResults_MonitoredApplications_MonitoredApplicatio~",
                        column: x => x.MonitoredApplicationId,
                        principalTable: "MonitoredApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthCheckRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Feature = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    MonitoringResultId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthCheckRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthCheckRecords_MonitoringResults_MonitoringResultId",
                        column: x => x.MonitoringResultId,
                        principalTable: "MonitoringResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthCheckRecords_MonitoringResultId",
                table: "HealthCheckRecords",
                column: "MonitoringResultId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringResults_MonitoredApplicationId",
                table: "MonitoringResults",
                column: "MonitoredApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HealthCheckRecords");

            migrationBuilder.DropTable(
                name: "MonitoringResults");

            migrationBuilder.DropTable(
                name: "MonitoredApplications");
        }
    }
}
