using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAM.T.Worker.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "MonitoredApplications",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "MonitoredApplications");
        }
    }
}
