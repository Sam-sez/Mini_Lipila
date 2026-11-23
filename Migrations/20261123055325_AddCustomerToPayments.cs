using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniLipila.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerToPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Payments",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Payments");
        }
    }
}
