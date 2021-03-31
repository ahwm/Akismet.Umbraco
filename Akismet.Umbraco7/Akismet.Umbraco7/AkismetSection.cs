using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Configuration.Dashboard;

namespace Akismet.Umbraco
{
    public class AkismetSection : ISection
    {
        /// <inheritdoc />
        public string Alias => "akismet";

        /// <inheritdoc />
        public string Name => "Akismet";

        public IEnumerable<string> Areas => throw new NotImplementedException();

        public IEnumerable<IDashboardTab> Tabs => throw new NotImplementedException();

        public IAccess AccessRights => throw new NotImplementedException();
    }
}