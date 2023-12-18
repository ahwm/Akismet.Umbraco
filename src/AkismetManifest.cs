using System.Collections.Generic;
using Umbraco.Cms.Core.Manifest;

namespace Akismet.Umbraco
{
    internal class AkismetManifest : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            var assembly = typeof(AkismetManifest).Assembly;

            manifests.Add(new PackageManifest
            {
                PackageName = "Akismet.Umbraco",
                Version = assembly.GetName()?.Version?.ToString(3) ?? "4.0.0",
                AllowPackageTelemetry = true,
                Scripts = new string[] {
                    "/App_Plugins/akismet/js/akismet.controller.js",
                    "/App_Plugins/akismet/js/dx.all.js",
                    "/App_Plugins/akismet/js/dx.aspnet.data.min.js"
                },
                Stylesheets = new string[]
                {
                    "/App_Plugins/akismet/css/akismet.css",
                    "/App_Plugins/akismet/css/dx.light.min.css"
                }
            });
        }
    }
}
