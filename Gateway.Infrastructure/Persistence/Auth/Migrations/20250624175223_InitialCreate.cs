using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Gateway.Infrastructure.Persistence.Auth.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "endpoints",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    controller = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    http_method = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_endpoints", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "endpoints_permissions",
                schema: "auth",
                columns: table => new
                {
                    endpoint_id = table.Column<long>(type: "bigint", nullable: false),
                    permission_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_endpoints_permissions", x => new { x.endpoint_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_endpoints_permissions_endpoints_endpoint_id",
                        column: x => x.endpoint_id,
                        principalSchema: "auth",
                        principalTable: "endpoints",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_endpoints_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalSchema: "auth",
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "auth",
                columns: table => new
                {
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    permission_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalSchema: "auth",
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "auth",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "auth",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "auth",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "auth",
                table: "permissions",
                columns: new[] { "id", "description", "name" },
                values: new object[] { 1L, "Do nothing", "do.nothing" });

            migrationBuilder.InsertData(
                schema: "auth",
                table: "roles",
                columns: new[] { "id", "is_admin", "name" },
                values: new object[] { 1L, true, "Admin" });

            migrationBuilder.InsertData(
                schema: "auth",
                table: "user_roles",
                columns: new[] { "role_id", "user_id" },
                values: new object[] { 1L, 1 });

            migrationBuilder.CreateIndex(
                name: "ix_endpoints_controller_action_http_method",
                schema: "auth",
                table: "endpoints",
                columns: new[] { "controller", "action", "http_method" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_endpoints_permissions_permission_id",
                schema: "auth",
                table: "endpoints_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_name",
                schema: "auth",
                table: "permissions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_id",
                schema: "auth",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                schema: "auth",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                schema: "auth",
                table: "user_roles",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "endpoints_permissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "endpoints",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "permissions",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "auth");
        }
    }
}
