using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace banhmihanhphuc.Migrations
{
    public partial class AddCustomerQrOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================
            // BẢNG YÊU CẦU ORDER TỪ KHÁCH
            // =========================================

            migrationBuilder.CreateTable(
                name: "customer_order_requests",
                columns: table => new
                {
                    id = table.Column<int>(
                        type: "integer",
                        nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy
                                .IdentityByDefaultColumn),

                    table_id = table.Column<int>(
                        type: "integer",
                        nullable: false),

                    status = table.Column<string>(
                        type: "text",
                        nullable: false),

                    created_at = table.Column<DateTime>(
                        type: "timestamp without time zone",
                        nullable: false),

                    accepted_at = table.Column<DateTime>(
                        type: "timestamp without time zone",
                        nullable: true),

                    rejected_at = table.Column<DateTime>(
                        type: "timestamp without time zone",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_customer_order_requests",
                        x => x.id);

                    table.ForeignKey(
                        name:
                            "FK_customer_order_requests_restaurant_tables_table_id",

                        column:
                            x => x.table_id,

                        principalTable:
                            "restaurant_tables",

                        principalColumn:
                            "id",

                        onDelete:
                            ReferentialAction.Restrict
                    );
                });


            // =========================================
            // BẢNG CHI TIẾT MÓN KHÁCH ORDER
            // =========================================

            migrationBuilder.CreateTable(
                name: "customer_order_request_details",
                columns: table => new
                {
                    id = table.Column<int>(
                        type: "integer",
                        nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy
                                .IdentityByDefaultColumn),

                    customer_order_request_id =
                        table.Column<int>(
                            type: "integer",
                            nullable: false),

                    food_id =
                        table.Column<int>(
                            type: "integer",
                            nullable: false),

                    quantity =
                        table.Column<int>(
                            type: "integer",
                            nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_customer_order_request_details",
                        x => x.id);


                    table.ForeignKey(
                        name:
                            "FK_customer_order_request_details_customer_order_requests_customer_order_request_id",

                        column:
                            x => x.customer_order_request_id,

                        principalTable:
                            "customer_order_requests",

                        principalColumn:
                            "id",

                        onDelete:
                            ReferentialAction.Cascade
                    );


                    table.ForeignKey(
                        name:
                            "FK_customer_order_request_details_foods_food_id",

                        column:
                            x => x.food_id,

                        principalTable:
                            "foods",

                        principalColumn:
                            "id",

                        onDelete:
                            ReferentialAction.Restrict
                    );
                });


            // =========================================
            // INDEX CHO BẢNG YÊU CẦU ORDER
            // =========================================

            migrationBuilder.CreateIndex(
                name:
                    "IX_customer_order_requests_table_id",

                table:
                    "customer_order_requests",

                column:
                    "table_id"
            );


            // =========================================
            // INDEX CHO CHI TIẾT ORDER
            // =========================================

            migrationBuilder.CreateIndex(
                name:
                    "IX_customer_order_request_details_customer_order_request_id",

                table:
                    "customer_order_request_details",

                column:
                    "customer_order_request_id"
            );


            migrationBuilder.CreateIndex(
                name:
                    "IX_customer_order_request_details_food_id",

                table:
                    "customer_order_request_details",

                column:
                    "food_id"
            );
        }


        protected override void Down(
            MigrationBuilder migrationBuilder)
        {
            // Xóa bảng chi tiết trước
            migrationBuilder.DropTable(
                name:
                    "customer_order_request_details"
            );


            // Sau đó xóa bảng yêu cầu chính
            migrationBuilder.DropTable(
                name:
                    "customer_order_requests"
            );
        }
    }
}