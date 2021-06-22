using Akismet.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Web.WebApi;

namespace Akismet.Umbraco.Controllers
{
    public class AkismetApiController : UmbracoAuthorizedApiController
    {
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
            var db = ApplicationContext.DatabaseContext.Database;

            var comments = db.Query<int>("SELECT COUNT(Id) FROM AkismetSubmission WHERE SpamStatus = @0", (int)SpamStatus.Spam).Single();

            return (int)Math.Ceiling(comments / 20m);
        }

        public IEnumerable<AkismetSubmission> GetSpamComments(int page = 1)
        {
            var db = ApplicationContext.DatabaseContext.Database;

            var comments = db.Query<AkismetSubmission>("SELECT * FROM AkismetSubmission WHERE SpamStatus = @0", (int)SpamStatus.Spam);

            return comments.Skip((page - 1) * 20).Take(20);
        }

        public int GetCommentPageCount()
        {
            var db = ApplicationContext.DatabaseContext.Database;

            var comments = db.Query<int>("SELECT COUNT(Id) FROM AkismetSubmission WHERE SpamStatus <> @0", (int)SpamStatus.Spam).Single();

            return (int)Math.Ceiling(comments / 20m);
        }

        public IEnumerable<AkismetSubmission> GetComments(int page = 1)
        {
            var db = ApplicationContext.DatabaseContext.Database;

            var comments = db.Query<AkismetSubmission>("SELECT * FROM AkismetSubmission WHERE SpamStatus <> @0", (int)SpamStatus.Spam);

            return comments.Skip((page - 1) * 20).Take(20);
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
            return GetStoredConfig();
        }

        public void DeleteComment(string id)
        {
            List<int> ids = id.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            var db = ApplicationContext.DatabaseContext.Database;

            db.Delete<AkismetSubmission>("DELETE FROM AkismetSubmission WHERE Id = @0", ids);
        }

        public void ReportHam(string id)
        {
            var config = AkismetService.GetConfig();
            List<int> ids = id.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            var db = ApplicationContext.DatabaseContext.Database;

            var comments = db.Query<AkismetSubmission>("SELECT * FROM AkismetSubmission WHERE Id = @0", ids).ToList();

            AkismetClient client = new AkismetClient(config["key"], new Uri(config["blogUrl"]), "Umbraco CMS");
            foreach (var comment in comments)
            {
                AkismetComment c = JsonConvert.DeserializeObject<AkismetComment>(comment.CommentData);
                c.CommentDate = comment.CommentDate.ToString("s");
                c.CommentType = comment.CommentType;
                client.SubmitHam(c);
            }

            DeleteComment(id);
        }

        public void ReportSpam(string id)
        {
            var config = AkismetService.GetConfig();
            List<int> ids = id.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            var db = ApplicationContext.DatabaseContext.Database;

            var comments = db.Query<AkismetSubmission>("SELECT * FROM AkismetSubmission WHERE Id = @0", ids).ToList();

            AkismetClient client = new AkismetClient(config["key"], new Uri(config["blogUrl"]), "Umbraco CMS");
            foreach (var comment in comments)
            {
                AkismetComment c = JsonConvert.DeserializeObject<AkismetComment>(comment.CommentData);
                c.CommentDate = comment.CommentDate.ToString("s");
                c.CommentType = comment.CommentType;
                client.SubmitSpam(c);
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
            var resp = client.GetStatistics();

            return resp;
        }

        internal Dictionary<string, string> GetStoredConfig()
        {
            string appData = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Plugins/akismet");
            if (!File.Exists(Path.Combine(appData, "akismetConfig.json")))
                return new Dictionary<string, string> { { "key", "" }, { "blogUrl", "" } };
            Dictionary<string, string> config = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(appData, "akismetConfig.json")));
            if (config == null)
                return new Dictionary<string, string> { { "key", "" }, { "blogUrl", "" } };

            return config;
        }
    }
}