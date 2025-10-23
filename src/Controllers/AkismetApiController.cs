using Akismet.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Akismet.Umbraco.Controllers
{
    // /Umbraco/backoffice/Api/AkismetApi
    public class AkismetApiController(AkismetService akismetService) : UmbracoAuthorizedApiController
    {
        public async Task<bool> VerifyKey(string blogUrl) => await akismetService.VerifyKeyAsync(blogUrl);

        public int GetSpamCommentPageCount() => akismetService.GetSpamCommentPageCount();

        public IEnumerable<AkismetSubmission> GetSpamComments(int page = 1) => akismetService.GetSpamComments(page);

        public int GetCommentPageCount() => akismetService.GetCommentPageCount();

        public IEnumerable<AkismetSubmission> GetComments(int page = 1) => akismetService.GetComments(page);

        public async Task<AkismetAccount> GetAccount(string blogUrl) => await akismetService.GetAccountAsync(blogUrl);

        public void DeleteComment(string id) => akismetService.DeleteComment(id);

        public async Task ReportHam(string id) => await akismetService.ReportHamAsync(id);

        public async Task ReportSpam(string id) => await akismetService.ReportSpamAsync(id);

        public async Task<dynamic> GetStats(string blogUrl) => await akismetService.GetStatsAsync(blogUrl);
    }
}
