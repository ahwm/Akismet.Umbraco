using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Akismet.Umbraco.Migrations
{
    public class AddAkismetCommentsTable(IMigrationContext context, ILogger<AddAkismetCommentsTable> logger) : AsyncMigrationBase(context)
    {
        protected override async Task MigrateAsync()
        {
            logger.LogDebug("Running migration {MigrationStep}", "AddAkismetCommentsTable");

            if (TableExists("AkismetSubmission") == false)
            {
                Create.Table<AkismetSubmissionSchema>().Do();
            }
            else
            {
                logger.LogDebug("The database table {DbTable} already exists, skipping", "AkismetSubmission");
            }
        }

        [TableName("AkismetSubmission")]
        [PrimaryKey("Id", AutoIncrement = true)]
        [ExplicitColumns]
        private class AkismetSubmissionSchema
        {
            [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
            [Column("Id")]
            public int Id { get; set; }

            [Column("CommentDate")]
            public DateTime CommentDate { get; set; }

            [Column("CommentType")]
            public string CommentType { get; set; } = string.Empty;

            [Column("CommentText")]
            [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
            public string CommentText { get; set; } = string.Empty;

            [Column("CommentData")]
            [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
            public string CommentData { get; set; } = string.Empty;

            [Column("Result")]
            public string Result { get; set; } = string.Empty;
        }
    }
}
