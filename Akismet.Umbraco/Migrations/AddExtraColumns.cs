using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Akismet.Umbraco.Migrations
{
    public class AddExtraColumns(IMigrationContext context, ILogger<AddExtraColumns> logger) : AsyncMigrationBase(context)
    {
        protected override async Task MigrateAsync()
        {
            logger.LogDebug("Running migration {MigrationStep}", "AddExtraColumns");

            if (!ColumnExists("AkismetSubmission", "SpamStatus"))
            {
                Create.Column("SpamStatus").OnTable("AkismetSubmission").AsInt32().WithDefaultValue(0).Do();
                Create.Column("UserIp").OnTable("AkismetSubmission").AsString(50).Nullable().Do();
                Create.Column("UserName").OnTable("AkismetSubmission").AsString(50).Nullable().Do();
            }
            else
            {
                logger.LogDebug("Additional columns already exist, skipping migration");
            }
        }
    }
}
