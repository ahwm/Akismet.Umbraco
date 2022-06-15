using Akismet.Net;
using System.Collections.Generic;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Akismet.Umbraco.Controllers
{
    public class AkismetApiController : UmbracoAuthorizedApiController
    {
        // /Umbraco/backoffice/Api/AkismetApi

        private readonly AkismetService AkismetService;

        public AkismetApiController(AkismetService akismetService)
        {
            AkismetService = akismetService;
        }

        public bool VerifyStoredKey()
        {
            return AkismetService.VerifyStoredKey();
        }

        public bool VerifyKey(string key, string blogUrl)
        {
            return AkismetService.VerifyKey(key, blogUrl);
        }

        public int GetSpamCommentPageCount()
        {
            return AkismetService.GetSpamCommentPageCount();
        }

        public IEnumerable<AkismetSubmission> GetSpamComments(int page = 1)
        {
            return AkismetService.GetSpamComments(page);
        }

        public int GetCommentPageCount()
        {
            return AkismetService.GetCommentPageCount();
        }

        public IEnumerable<AkismetSubmission> GetComments(int page = 1)
        {
            return AkismetService.GetComments(page);
        }

        public AkismetAccount GetAccount()
        {
            return AkismetService.GetAccount();
        }

        public void DeleteComment(string id)
        {
            AkismetService.DeleteComment(id);
        }

        public void ReportHam(string id)
        {
            AkismetService.ReportHam(id);
        }

        public void ReportSpam(string id)
        {
            AkismetService.ReportSpam(id);
        }

        public dynamic GetStats()
        {
            return AkismetService.GetStats();
        }
    }
}
