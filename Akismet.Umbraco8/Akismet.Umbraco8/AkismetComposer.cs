using NPoco;
using System;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Sections;

namespace Akismet.Umbraco
{
    public class AkismetUserComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Sections().InsertBefore<PackagesSection, AkismetSection>();
        }
    }

    public class AkismetComposer : ComponentComposer<AkismetComonent>
    { }

    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class AkismetComonent : IComponent
    {
        private IScopeProvider _scopeProvider;
        private IMigrationBuilder _migrationBuilder;
        private IKeyValueService _keyValueService;
        private ILogger _logger;

        public AkismetComonent(IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, ILogger logger)
        {
            _scopeProvider = scopeProvider;
            _migrationBuilder = migrationBuilder;
            _keyValueService = keyValueService;
            _logger = logger;
        }

        public void Initialize()
        {
            // Create a migration plan for a specific project/feature
            // We can then track that latest migration state/step for this project/feature
            var migrationPlan = new MigrationPlan("AkismetComments");

            // This is the steps we need to take
            // Each step in the migration adds a unique value
            migrationPlan.From(string.Empty)
                .To<AddAkismetCommentsTable>("akismetcomments-db");

            // Go and upgrade our site (Will check if it needs to do the work or not)
            // Based on the current/latest step
            var upgrader = new Upgrader(migrationPlan);
            upgrader.Execute(_scopeProvider, _migrationBuilder, _keyValueService, _logger);
        }

        public void Terminate()
        {

        }
    }

    public class AddAkismetCommentsTable : MigrationBase
    {
        public AddAkismetCommentsTable(IMigrationContext context) : base(context)
        { }

        public override void Migrate()
        {
            Logger.Debug<AddAkismetCommentsTable>("Running migration {MigrationStep}", "AddAkismetCommentsTable");

            // Lots of methods available in the MigrationBase class - discover with this.
            if (TableExists("AkismetComments") == false)
            {
                Create.Table<AkismetCommentsSchema>().Do();
            }
            else
            {
                Logger.Debug<AddAkismetCommentsTable>("The database table {DbTable} already exists, skipping", "AkismetComments");
            }
        }

        [TableName("AkismetComments")]
        [PrimaryKey("Id", AutoIncrement = true)]
        [ExplicitColumns]
        public class AkismetCommentsSchema
        {
            [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
            [Column("Id")]
            public int Id { get; set; }

            [Column("CommentDate")]
            public DateTime CommentDate { get; set; }

            [Column("CommentType")]
            public string CommentType { get; set; }

            [Column("CommentData")]
            [SpecialDbType(SpecialDbTypes.NTEXT)]
            public string CommentData { get; set; }
        }
    }
}