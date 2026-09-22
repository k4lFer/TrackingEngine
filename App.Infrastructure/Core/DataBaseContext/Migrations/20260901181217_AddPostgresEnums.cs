using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Core.DataBaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddPostgresEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:auth.user_role_enum", "member,moderator,admin")
                .Annotation("Npgsql:Enum:auth.user_status_enum", "active,inactive,suspended")
                .Annotation("Npgsql:Enum:notifications.notification_type_enum", "info,success,warning,error")
                .Annotation("Npgsql:Enum:user_credential.verification_type_enum", "email_confirmation,password_reset");

            migrationBuilder.Sql(
                "ALTER TABLE notifications.notifications ALTER COLUMN type TYPE notifications.notification_type_enum USING type::notifications.notification_type_enum;");

            migrationBuilder.Sql(
                "ALTER TABLE user_credential.email_verifications ALTER COLUMN type TYPE user_credential.verification_type_enum USING type::user_credential.verification_type_enum;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:auth.user_role_enum", "member,moderator,admin")
                .OldAnnotation("Npgsql:Enum:auth.user_status_enum", "active,inactive,suspended")
                .OldAnnotation("Npgsql:Enum:notifications.notification_type_enum", "info,success,warning,error")
                .OldAnnotation("Npgsql:Enum:user_credential.verification_type_enum", "email_confirmation,password_reset");

            migrationBuilder.Sql(
                "ALTER TABLE notifications.notifications ALTER COLUMN type TYPE varchar(50) USING type::varchar;");

            migrationBuilder.Sql(
                "ALTER TABLE user_credential.email_verifications ALTER COLUMN type TYPE varchar(30) USING type::varchar;");
        }
    }
}
