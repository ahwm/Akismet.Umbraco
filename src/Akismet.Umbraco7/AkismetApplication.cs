using Akismet.Umbraco;
using Akismet.Umbraco.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using umbraco.businesslogic;
using umbraco.interfaces;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web;

namespace Akismet.Umbraco7
{
    [Application("akismet", "Akismet", "icon-chat-active", sortOrder: 7)]
    public class AkismetApplication : IApplication
    {
    }

    public class AkismetUmbracoApplication : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //Get the Umbraco Database context
            var dbCtx = applicationContext.DatabaseContext;
            var db = new DatabaseSchemaHelper(dbCtx.Database, applicationContext.ProfilingLogger.Logger, dbCtx.SqlSyntax);

            //Create DB table - and set overwrite to false
            db.CreateTable<AkismetSubmission>(false);
        }
    }
}
