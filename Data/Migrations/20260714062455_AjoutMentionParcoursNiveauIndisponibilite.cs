using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GestionSallesEmit.Data.Migrations
{
    /// <inheritdoc />
    public partial class AjoutMentionParcoursNiveauIndisponibilite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NomSalle",
                table: "Salles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Prenom",
                table: "Enseignants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Enseignants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Enseignants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "NomCours",
                table: "Cours",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Niveau",
                table: "Cours",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ParcoursId",
                table: "Cours",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Indisponibilites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnseignantId = table.Column<int>(type: "integer", nullable: false),
                    Jour = table.Column<string>(type: "text", nullable: false),
                    HeureDebut = table.Column<string>(type: "text", nullable: false),
                    HeureFin = table.Column<string>(type: "text", nullable: false),
                    Raison = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indisponibilites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Indisponibilites_Enseignants_EnseignantId",
                        column: x => x.EnseignantId,
                        principalTable: "Enseignants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mentions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomMention = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mentions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parcours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomParcours = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MentionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parcours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parcours_Mentions_MentionId",
                        column: x => x.MentionId,
                        principalTable: "Mentions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cours_ParcoursId",
                table: "Cours",
                column: "ParcoursId");

            migrationBuilder.CreateIndex(
                name: "IX_Indisponibilites_EnseignantId",
                table: "Indisponibilites",
                column: "EnseignantId");

            migrationBuilder.CreateIndex(
                name: "IX_Parcours_MentionId",
                table: "Parcours",
                column: "MentionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cours_Parcours_ParcoursId",
                table: "Cours",
                column: "ParcoursId",
                principalTable: "Parcours",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cours_Parcours_ParcoursId",
                table: "Cours");

            migrationBuilder.DropTable(
                name: "Indisponibilites");

            migrationBuilder.DropTable(
                name: "Parcours");

            migrationBuilder.DropTable(
                name: "Mentions");

            migrationBuilder.DropIndex(
                name: "IX_Cours_ParcoursId",
                table: "Cours");

            migrationBuilder.DropColumn(
                name: "Niveau",
                table: "Cours");

            migrationBuilder.DropColumn(
                name: "ParcoursId",
                table: "Cours");

            migrationBuilder.AlterColumn<string>(
                name: "NomSalle",
                table: "Salles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Prenom",
                table: "Enseignants",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Nom",
                table: "Enseignants",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Enseignants",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "NomCours",
                table: "Cours",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
