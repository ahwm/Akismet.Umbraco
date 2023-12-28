using Akismet.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Akismet.Umbraco.Controllers
{
    // /Umbraco/backoffice/Api/AkismetApi
    public class AkismetApiController(AkismetService akismetService) : UmbracoAuthorizedApiController
    {
        private readonly AkismetService AkismetService = akismetService;

        public Dictionary<string, string> GetConfig() => AkismetService.GetConfig();

        public async Task<bool> VerifyStoredKey() => await AkismetService.VerifyStoredKeyAsync();

        public async Task<bool> VerifyKey(string key, string blogUrl) => await AkismetService.VerifyKeyAsync(key, blogUrl);

        public int GetSpamCommentPageCount() => AkismetService.GetSpamCommentPageCount();

        public IEnumerable<AkismetSubmission> GetSpamComments(int page = 1) => AkismetService.GetSpamComments(page);

        public int GetCommentPageCount() => AkismetService.GetCommentPageCount();

        public IEnumerable<AkismetSubmission> GetComments(int page = 1) => AkismetService.GetComments(page);

        public async Task<AkismetAccount> GetAccount() => await AkismetService.GetAccountAsync();

        public void DeleteComment(string id) => AkismetService.DeleteComment(id);

        public async Task ReportHam(string id) => await AkismetService.ReportHamAsync(id);

        public async Task ReportSpam(string id) => await AkismetService.ReportSpamAsync(id);

        public async Task<dynamic> GetStats() => await AkismetService.GetStatsAsync();
    }
}
