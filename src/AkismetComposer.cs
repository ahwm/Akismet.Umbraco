using Microsoft.Extensions.Logging;
using NPoco;
using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Sections;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Akismet.Umbraco
{
    public class AkismetUserComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Sections().InsertBefore<PackagesSection, AkismetSection>();
            builder.AddAkismet();
        }
    }

    public class AkismetComponent : IComponent
    {
        private readonly ICoreScopeProvider _coreScopeProvider;
        private readonly IKeyValueService _keyValueService;
        private readonly IRuntimeState _runtimeState;
        private readonly IMigrationPlanExecutor _migrationPlanExecutor;

        public AkismetComponent(ICoreScopeProvider coreScopeProvider,
            IKeyValueService keyValueService,
            IRuntimeState runtimeState,
            IMigrationPlanExecutor migrationPlanExecutor)
        {
            _coreScopeProvider = coreScopeProvider;
            _keyValueService = keyValueService;
            _runtimeState = runtimeState;
            _migrationPlanExecutor = migrationPlanExecutor;
        }

        public void Initialize()
        {
            if (_runtimeState.Level < RuntimeLevel.Run)
            {
                return;
            }

            // Create a migration plan for a specific project/feature
            // We can then track that latest migration state/step for this project/feature
            var migrationPlan = new MigrationPlan("AkismetComments");

            // This is the steps we need to take
            // Each step in the migration adds a unique value
            migrationPlan.From(string.Empty).To<AddAkismetCommentsTable>("akismetcomments-db");
            migrationPlan.From("akismetcomments-db").To<AddExtraColumns>("akismetcomments-AddExtraColumns");

            // Go and upgrade our site (Will check if it needs to do the work or not)
            // Based on the current/latest step
            var upgrader = new Upgrader(migrationPlan);
            upgrader.Execute(_migrationPlanExecutor, _coreScopeProvider, _keyValueService);
        }

        public void Terminate()
        {

        }
    }

    public class AddAkismetCommentsTable : MigrationBase
    {
        public AddAkismetCommentsTable(IMigrationContext context) : base(context)
        { }

        protected override void Migrate()
        {
            Logger.LogDebug("Running migration {MigrationStep}", "AddAkismetCommentsTable");

            // Lots of methods available in the MigrationBase class - discover with this.
            if (TableExists("AkismetComments") == false)
            {
                Create.Table<AkismetSubmissionSchema>().Do();
            }
            else
            {
                Logger.LogDebug("The database table {DbTable} already exists, skipping", "AkismetComments");
            }
        }

        [TableName("AkismetSubmission")]
        [PrimaryKey("Id", AutoIncrement = true)]
        [ExplicitColumns]
        public class AkismetSubmissionSchema
        {
            [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
            [Column("Id")]
            public int Id { get; set; }

            [Column("CommentDate")]
            public DateTime CommentDate { get; set; }

            [Column("CommentType")]
            public string CommentType { get; set; }

            [Column("CommentText")]
            [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
            public string CommentText { get; set; }

            [Column("CommentData")]
            [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
            public string CommentData { get; set; }

            [Column("Result")]
            public string Result { get; set; }
        }
    }

    public class AddExtraColumns : MigrationBase
    {
        public AddExtraColumns(IMigrationContext context) : base(context) { }

        protected override void Migrate()
        {
            Logger.LogDebug("Running migration {MigrationStep}", "AddExtraColumns");

            if (!ColumnExists("AkismetSubmission", "SpamStatus"))
            {
                Create.Column("SpamStatus").OnTable("AkismetSubmission").AsInt32().WithDefaultValue(0).Do();
                Create.Column("UserIp").OnTable("AkismetSubmission").AsString(50).Nullable().Do();
                Create.Column("UserName").OnTable("AkismetSubmission").AsString(50).Nullable().Do();
            }
            else
            {
                Logger.LogDebug("Additional columns (checked for Location) already exist, skipping migration");
            }
        }
    }

    [TableName("AkismetSubmission")]
    [PrimaryKey("Id", AutoIncrement = true)]
    [ExplicitColumns]
    public class AkismetSubmission
    {
        [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("CommentDate")]
        public DateTime CommentDate { get; set; }

        [Column("CommentType")]
        public string CommentType { get; set; }

        [Column("CommentText")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string CommentText { get; set; }

        [Column("CommentData")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string CommentData { get; set; }

        [Column("Result")]
        public string Result { get; set; }

        [Column("SpamStatus")]
        public int SpamStatus { get; set; }

        [Column("UserIp")]
        public string UserIp { get; set; }

        [Column("UserName")]
        public string UserName { get; set; }
    }
}