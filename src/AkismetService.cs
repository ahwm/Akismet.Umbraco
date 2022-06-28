using Akismet.Net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Akismet.Umbraco
{
    public static class ServicesConfiguration
    {
        public static IUmbracoBuilder AddAkismet(this IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<AkismetService>();

            return builder;
        }
    }

    public class AkismetService
    {
        private readonly IScopeProvider scopeProvider;
        private string apiKey = "";
        private string blogUrl = "";

        public AkismetService(IScopeProvider provider)
        {
            scopeProvider = provider;

            string appData = MapPath(AppDomain.CurrentDomain, "~/App_Plugins/akismet");
            if (File.Exists(Path.Combine(appData, "akismetConfig.json")))
            {
                Dictionary<string, string> config = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(appData, "akismetConfig.json")));
                apiKey = config["key"];
                blogUrl = config["blogUrl"];
            }
        }

        public Dictionary<string, string> GetConfig()
        {
            return new Dictionary<string, string> { { "key", apiKey }, { "blogUrl", blogUrl } };
        }

        internal void SetConfig(string key, string blogUrl)
        {
            string appData = MapPath(AppDomain.CurrentDomain, "~/App_Plugins/akismet");
            var config = new Dictionary<string, string> { { "key", key }, { "blogUrl", blogUrl } };
            File.WriteAllText(Path.Combine(appData, "akismetConfig.json"), JsonConvert.SerializeObject(config));
            apiKey = key;
            this.blogUrl = blogUrl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool VerifyStoredKey()
        {
            if (String.IsNullOrWhiteSpace(apiKey) || String.IsNullOrWhiteSpace(blogUrl))
                return false;

            AkismetClient client = new AkismetClient(apiKey, new Uri(blogUrl), "Umbraco CMS");
            return client.VerifyKey();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="blogUrl"></param>
        /// <returns></returns>
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
                    SetConfig(key, blogUrl);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public AkismetAccount GetAccount()
        {
            if (String.IsNullOrWhiteSpace(apiKey) || String.IsNullOrWhiteSpace(blogUrl))
                return new AkismetAccount();

            AkismetClient client = new AkismetClient(apiKey, new Uri(blogUrl), "Umbraco CMS");
            return client.GetAccountStatus();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="comment">Comment object to be checked</param>
        /// <param name="useStrict">Set to True to only send designated Ham, false to include Unspecified</param>
        /// <returns></returns>
        public bool CheckComment(AkismetComment comment, bool useStrict = false)
        {
            if (String.IsNullOrWhiteSpace(apiKey) || String.IsNullOrWhiteSpace(blogUrl))
                return true;

            var client = new AkismetClient(apiKey, new Uri(blogUrl), "Umbraco CMS");
            var result = client.Check(comment);

            if (result.ProTip != "discard")
            {
                using (var scope = scopeProvider.CreateScope())
                {
                    var sql = scope.Database.Insert(new AkismetSubmission
                    {
                        CommentData = JsonConvert.SerializeObject(comment),
                        CommentDate = DateTime.UtcNow,
                        CommentText = comment.CommentContent,
                        CommentType = comment.CommentType.ToString(),
                        Result = JsonConvert.SerializeObject(result),
                        SpamStatus = (int)result.SpamStatus,
                        UserIp = comment.UserIp,
                        UserName = comment.CommentAuthor
                    });

                    scope.Complete();
                }
            }

            if (useStrict)
                return result.SpamStatus == SpamStatus.Ham;
            else
                return result.SpamStatus != SpamStatus.Spam;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetSpamCommentPageCount()
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .SelectCount<AkismetSubmission>(x => x.Id).From("AkismetSubmission").Where<AkismetSubmission>(x => x.SpamStatus == (int)SpamStatus.Spam);

                return (int)Math.Ceiling(scope.Database.ExecuteScalar<int>(sql) / 20m);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public IEnumerable<AkismetSubmission> GetSpamComments(int page = 1)
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("AkismetSubmission").Where<AkismetSubmission>(x => x.SpamStatus == (int)SpamStatus.Spam);

                return scope.Database.Query<AkismetSubmission>(sql).Skip((page - 1) * 20).Take(20);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetCommentPageCount()
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .SelectCount<AkismetSubmission>(x => x.Id).From("AkismetSubmission").Where<AkismetSubmission>(x => x.SpamStatus != (int)SpamStatus.Spam);

                return (int)Math.Ceiling(scope.Database.ExecuteScalar<int>(sql) / 20m);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public IEnumerable<AkismetSubmission> GetComments(int page = 1)
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("AkismetSubmission").Where<AkismetSubmission>(x => x.SpamStatus != (int)SpamStatus.Spam);

                return scope.Database.Query<AkismetSubmission>(sql).Skip((page - 1) * 20).Take(20);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void ReportHam(string id)
        {
            List<int> ids = id.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("AkismetSubmission").Where<AkismetSubmission>(x => ids.Contains(x.Id));

                var comments = scope.Database.Query<AkismetSubmission>(sql).ToList();

                AkismetClient client = new AkismetClient(apiKey, new Uri(blogUrl), "Umbraco CMS");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void ReportSpam(string id)
        {
            List<int> ids = id.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("AkismetSubmission").Where<AkismetSubmission>(x => ids.Contains(x.Id));

                var comments = scope.Database.Query<AkismetSubmission>(sql).ToList();

                AkismetClient client = new AkismetClient(apiKey, new Uri(blogUrl), "Umbraco CMS");
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public dynamic GetStats()
        {
            if (String.IsNullOrWhiteSpace(apiKey) || String.IsNullOrWhiteSpace(blogUrl))
                return null;

            AkismetClient client = new AkismetClient(apiKey, new Uri(blogUrl), "Umbraco CMS");
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

        internal static string MapPath(AppDomain domain, string path)
        {
            if (!path.StartsWith("~/"))
                return path;

            return Path.Combine(domain.BaseDirectory, path.Replace("~/", ""));
        }
    }
}