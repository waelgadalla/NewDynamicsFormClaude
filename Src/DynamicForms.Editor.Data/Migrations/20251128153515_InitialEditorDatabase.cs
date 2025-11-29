using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamicForms.Editor.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialEditorDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EditorConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ConfigType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EditorFormModules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TitleFr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    DescriptionFr = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    SchemaJson = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorFormModules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EditorHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EditorSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    SnapshotJson = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    ActionDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EditorWorkflows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TitleFr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    SchemaJson = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorWorkflows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublishedFormModules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TitleFr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SchemaJson = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    PublishedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishedFormModules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublishedWorkflows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TitleFr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SchemaJson = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    PublishedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublishedWorkflows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EditorConfiguration_ConfigKey",
                table: "EditorConfiguration",
                column: "ConfigKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EditorFormModules_ModifiedAt",
                table: "EditorFormModules",
                column: "ModifiedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_EditorFormModules_ModuleId",
                table: "EditorFormModules",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_EditorFormModules_Status",
                table: "EditorFormModules",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EditorHistory_CreatedAt",
                table: "EditorHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EditorHistory_EditorSessionId",
                table: "EditorHistory",
                columns: new[] { "EditorSessionId", "SequenceNumber" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_EditorHistory_EntityTypeId",
                table: "EditorHistory",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_EditorWorkflows_Status",
                table: "EditorWorkflows",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EditorWorkflows_WorkflowId",
                table: "EditorWorkflows",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_PublishedFormModules_IsActive",
                table: "PublishedFormModules",
                column: "IsActive",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PublishedFormModules_ModuleId_Version",
                table: "PublishedFormModules",
                columns: new[] { "ModuleId", "Version" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PublishedWorkflows_IsActive",
                table: "PublishedWorkflows",
                column: "IsActive",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PublishedWorkflows_WorkflowId_Version",
                table: "PublishedWorkflows",
                columns: new[] { "WorkflowId", "Version" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EditorConfiguration");

            migrationBuilder.DropTable(
                name: "EditorFormModules");

            migrationBuilder.DropTable(
                name: "EditorHistory");

            migrationBuilder.DropTable(
                name: "EditorWorkflows");

            migrationBuilder.DropTable(
                name: "PublishedFormModules");

            migrationBuilder.DropTable(
                name: "PublishedWorkflows");
        }
    }
}
