using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NationalBank.Migrations
{
    /// <inheritdoc />
    public partial class SeedBranchesAndManagers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Branches",
                columns: new[] { "Code", "Address", "Name", "PhoneNumber" },
                values: new object[,]
                {
                    { "CAI-01", "15 Tahrir Square, Cairo", "Cairo Main Branch", "0223456789" },
                    { "ALX-01", "40 Fouad Street, Alexandria", "Alexandria Branch", "034876543" },
                    { "GIZ-01", "120 Haram Street, Giza", "Giza Branch", "0235678901" }
                });

            migrationBuilder.InsertData(
                table: "Managers",
                columns: new[] { "Id", "BranchCode", "Email", "FullName", "HireDate", "PhoneNumber" },
                values: new object[,]
                {
                    { 1, "CAI-01", "khaled.mostafa@nbank.com", "Khaled Mostafa", new DateTime(2015, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "01001234567" },
                    { 2, "ALX-01", "dina.samir@nbank.com", "Dina Samir", new DateTime(2018, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "01112345678" },
                    { 3, "GIZ-01", "hany.fawzy@nbank.com", "Hany Fawzy", new DateTime(2020, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "01223456789" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Managers",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3 });

            migrationBuilder.DeleteData(
                table: "Branches",
                keyColumn: "Code",
                keyValues: new object[] { "ALX-01", "CAI-01", "GIZ-01" });
        }
    }
}
