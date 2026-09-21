using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookDriver.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingsAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "DriverProfiles",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "RatingCount",
                table: "DriverProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CustomerRating",
                table: "Bookings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerRatingComment",
                table: "Bookings",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DriverRating",
                table: "Bookings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverRatingComment",
                table: "Bookings",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "RatingCount",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "CustomerRating",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CustomerRatingComment",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DriverRating",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DriverRatingComment",
                table: "Bookings");
        }
    }
}
