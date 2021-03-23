using Akismet.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Web.WebApi;

namespace Akismet.Umbraco.Controllers
{
    public class AkismetApiController : UmbracoAuthorizedApiController
    {
        // /Umbraco/backoffice/Api/AkismetApi

        private readonly AkismetService AkismetService;
        private readonly IScopeProvider scopeProvider;

        public AkismetApiController(IScopeProvider scopeProvider, AkismetService akismetService)
        {
            this.scopeProvider = scopeProvider;
            AkismetService = akismetService;
        }

        public bool VerifyKey()
        {
            string key, blogUrl;
            var config = AkismetService.GetConfig();
            key = config["key"];
            blogUrl = config["blogUrl"];
            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(blogUrl))
                return false;

            AkismetClient client = new AkismetClient(key, new Uri(blogUrl), "Umbraco CMS");
            return client.VerifyKey();
        }
        
        public bool VerifyKey(string key, string blogUrl)
        {
            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(blogUrl))
                return false;

            if (key.Length != 12)
                return false;

            AkismetClient client = new AkismetClient(key, new Uri(blogUrl), "Umbraco CMS");
            return client.VerifyKey();
        }

        public int GetCommentPageCount()
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .SelectCount<AkismetSubmission>(x => x.Id).From("AkismetSubmission");

                return (int)Math.Ceiling(scope.Database.ExecuteScalar<int>(sql) / 20m);
            }
        }

        public IEnumerable<AkismetSubmission> GetComments(int page = 1)
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("AkismetSubmission");

                return scope.Database.Query<AkismetSubmission>(sql).Skip((page - 1) * 20).Take(20);
            }
        }

        public AkismetAccount GetAccount()
        {
            string key, blogUrl;
            var config = AkismetService.GetConfig();
            key = config["key"];
            blogUrl = config["blogUrl"];
            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(blogUrl))
                return new AkismetAccount();

            AkismetClient client = new AkismetClient(key, new Uri(blogUrl), "Umbraco CMS");
            return client.GetAccountStatus();
        }

        public Dictionary<string, string> GetConfig()
        {
            return AkismetService.GetConfig();
        }

        public void DeleteComment(int id)
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: false))
            {
                var sql = scope.SqlContext.Sql()
                    .Delete().From("AkismetSubmission").Where<AkismetSubmission>(x => x.Id == id);

                scope.Database.Execute(sql);

                scope.Complete();
            }
        }

        public dynamic GetStats()
        {
            string key, blogUrl;
            var config = AkismetService.GetConfig();
            key = config["key"];
            blogUrl = config["blogUrl"];
            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(blogUrl))
                return null;

            AkismetClient client = new AkismetClient(key, new Uri(blogUrl), "Umbraco CMS");
            var resp = client.GetStatistics();

            return resp;
        }
    }
}