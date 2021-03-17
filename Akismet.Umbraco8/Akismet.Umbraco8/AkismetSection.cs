using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Models.Sections;

namespace Akismet.Umbraco
{
    public class AkismetSection : ISection
    {
        /// <inheritdoc />
        public string Alias => "akismet";

        /// <inheritdoc />
        public string Name => "Akismet";
    }
}