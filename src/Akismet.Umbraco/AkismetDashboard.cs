using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Cms.Core.Dashboards;

namespace Akismet.Umbraco
{
    public class AkismetDashboard : IDashboard
    {
        public string Alias => "akismetDashboard";

        public string[] Sections => new[]
        {
            "akismet"
        };

        public string View => "/App_Plugins/akismet/overview.html";

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}