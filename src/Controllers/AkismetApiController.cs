using Akismet.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Extensions;

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

        public bool VerifyStoredKey()
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
            bool isValid = client.VerifyKey();
            if (isValid)
            {
                try
                {
                    AkismetService.SetConfig(key, blogUrl);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public int GetSpamCommentPageCount()
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .SelectCount<AkismetSubmission>(x => x.Id).From("AkismetSubmission").Where<AkismetSubmission>(x => x.SpamStatus == (int)SpamStatus.Spam);

                return (int)Math.Ceiling(scope.Database.ExecuteScalar<int>(sql) / 20m);
            }
        }

        public IEnumerable<AkismetSubmission> GetSpamComments(int page = 1)
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("AkismetSubmission").Where<AkismetSubmission>(x => x.SpamStatus == (int)SpamStatus.Spam);

                return scope.Database.Query<AkismetSubmission>(sql).Skip((page - 1) * 20).Take(20);
            }
        }

        public int GetCommentPageCount()
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .SelectCount<AkismetSubmission>(x => x.Id).From("AkismetSubmission").Where<AkismetSubmission>(x => x.SpamStatus != (int)SpamStatus.Spam);

                return (int)Math.Ceiling(scope.Database.ExecuteScalar<int>(sql) / 20m);
            }
        }

        public IEnumerable<AkismetSubmission> GetComments(int page = 1)
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("AkismetSubmission").Where<AkismetSubmission>(x => x.SpamStatus != (int)SpamStatus.Spam);

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

        public void DeleteComment(string id)
        {
            List<int> ids = id.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Delete().From("AkismetSubmission").Where<AkismetSubmission>(x => ids.Contains(x.Id));

                scope.Database.Execute(sql);

                scope.Complete();
            }
        }

        public void ReportHam(string id)
        {
            var config = AkismetService.GetConfig();
            List<int> ids = id.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("AkismetSubmission").Where<AkismetSubmission>(x => ids.Contains(x.Id));

                var comments = scope.Database.Query<AkismetSubmission>(sql).ToList();

                AkismetClient client = new AkismetClient(config["key"], new Uri(config["blogUrl"]), "Umbraco CMS");
                foreach (var comment in comments)
                {
                    AkismetComment c = JsonConvert.DeserializeObject<AkismetComment>(comment.CommentData);
                    c.CommentDate = comment.CommentDate.ToString("s");
                    c.CommentType = comment.CommentType;
                    client.SubmitHam(c);
                }


                sql = scope.SqlContext.Sql()
                    .Update<AkismetSubmission>().From("AkismetSubmission").Where<AkismetSubmission>(x => ids.Contains(x.Id));
            }
            DeleteComment(id);
        }

        public void ReportSpam(string id)
        {
            var config = AkismetService.GetConfig();
            List<int> ids = id.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("AkismetSubmission").Where<AkismetSubmission>(x => ids.Contains(x.Id));

                var comments = scope.Database.Query<AkismetSubmission>(sql).ToList();

                AkismetClient client = new AkismetClient(config["key"], new Uri(config["blogUrl"]), "Umbraco CMS");
                foreach (var comment in comments)
                {
                    AkismetComment c = JsonConvert.DeserializeObject<AkismetComment>(comment.CommentData);
                    c.CommentDate = comment.CommentDate.ToString("s");
                    c.CommentType = comment.CommentType;
                    client.SubmitSpam(c);
                }
            }
            DeleteComment(id);
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
            var respAll = client.GetStatistics("all");
            var resp = client.GetStatistics();
            resp.Spam = respAll.Spam;
            resp.Ham = respAll.Ham;
            resp.MissedSpam = respAll.MissedSpam;
            resp.FalsePositives = respAll.FalsePositives;
            resp.TimeSaved = respAll.TimeSaved;
            resp.Accuracy = respAll.Accuracy;

            return resp;
        }
    }
}
