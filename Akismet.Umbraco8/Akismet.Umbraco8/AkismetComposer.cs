using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Composing;
using Umbraco.Web;
using Umbraco.Web.Sections;

namespace Akismet.Umbraco
{
    public class AkismetComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Sections().InsertBefore<PackagesSection, AkismetSection>();
        }
    }
}