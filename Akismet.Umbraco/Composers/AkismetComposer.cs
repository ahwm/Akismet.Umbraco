using Akismet.Net;
using Akismet.Umbraco.Migrations;
using Akismet.Umbraco.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Akismet.Umbraco.Composers
{
    public class AkismetComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // Register AkismetApi.Net client with DI
            var apiKey = builder.Config["Akismet:ApiKey"] ?? string.Empty;
            builder.Services.AddAkismet(apiKey, "Umbraco CMS");

            // Register the AkismetService
            builder.Services.AddScoped<AkismetService>();

            // Register the migration notification handler
            builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, AkismetMigrationHandler>();
        }
    }

    public class AkismetMigrationHandler(IMigrationPlanExecutor migrationPlanExecutor, ICoreScopeProvider coreScopeProvider, IKeyValueService keyValueService) : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
    {
        public async Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
        {
            var migrationPlan = new MigrationPlan("Akismet.Umbraco");

            migrationPlan.From(string.Empty)
                .To<AddAkismetCommentsTable>("akismet-db")
                .To<AddExtraColumns>("akismet-extra-columns");

            var upgrader = new Upgrader(migrationPlan);
            await upgrader.ExecuteAsync(migrationPlanExecutor, coreScopeProvider, keyValueService);
        }
    }
}
