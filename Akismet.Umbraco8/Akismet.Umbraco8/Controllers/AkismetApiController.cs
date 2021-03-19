using Akismet.Net;
using System;
using Umbraco.Web.WebApi;

namespace Akismet.Umbraco.Controllers
{
    public class AkismetApiController : UmbracoApiController
    {
        public bool VerifyKey()
        {
            AkismetClient client = new AkismetClient("", new Uri(""), "Umbraco CMS");
            return client.VerifyKey();
        }
    }
}