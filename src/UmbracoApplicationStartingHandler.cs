using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Akismet.Umbraco.Notifications.Handlers
{
    public class AkismetCommentsMigration(IScopeProvider scopeProvider, IMigrationPlanExecutor migrationPlanExecutor, IKeyValueService keyValueService, IRuntimeState runtimeState) : INotificationHandler<UmbracoApplicationStartingNotification>
    {
        private readonly IScopeProvider _scopeProvider = scopeProvider;
        private readonly IMigrationPlanExecutor _migrationPlanExecutor = migrationPlanExecutor;
        private readonly IKeyValueService _keyValueService = keyValueService;
        private readonly IRuntimeState _runtimeState = runtimeState;

        public void Handle(UmbracoApplicationStartingNotification notification)
        {
            if (_runtimeState.Level >= RuntimeLevel.Run)
            {
                MigrationPlan migrationPlan = new MigrationPlan("AkismetComments");
                migrationPlan.From(string.Empty).To<AddAkismetCommentsTable>("akismetcomments-db")
                    .To<AddExtraColumns>("akismetcomments-AddExtraColumns");
                new Upgrader(migrationPlan).Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService);
            }
        }
    }
}
