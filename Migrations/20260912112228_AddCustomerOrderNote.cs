using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace banhmihanhphuc.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerOrderNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "customer_order_requests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "note",
                table: "customer_order_requests");
        }
    }
}
