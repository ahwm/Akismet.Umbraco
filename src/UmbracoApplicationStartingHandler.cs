using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Akismet.Umbraco.Notifications.Handlers
{
    public class AkismetCommentsMigration : INotificationHandler<UmbracoApplicationStartingNotification>
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IMigrationPlanExecutor _migrationPlanExecutor;
        private readonly IKeyValueService _keyValueService;
        private readonly IRuntimeState _runtimeState;

        public AkismetCommentsMigration(IScopeProvider scopeProvider, IMigrationPlanExecutor migrationPlanExecutor, IKeyValueService keyValueService, IRuntimeState runtimeState)
        {
            _scopeProvider = scopeProvider;
            _migrationPlanExecutor = migrationPlanExecutor;
            _keyValueService = keyValueService;
            _runtimeState = runtimeState;
        }

        public void Handle(UmbracoApplicationStartingNotification notification)
        {
            if (_runtimeState.Level >= RuntimeLevel.Run)
            {
                MigrationPlan migrationPlan = new MigrationPlan("Skybrud.Umbraco.Redirects");
                migrationPlan.From(string.Empty).To<AddAkismetCommentsTable>("akismetcomments-db")
                    .To<AddExtraColumns>("akismetcomments-AddExtraColumns");
                new Upgrader(migrationPlan).Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService);
            }
        }
    }
}
