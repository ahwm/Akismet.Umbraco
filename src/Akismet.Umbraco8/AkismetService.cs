﻿using Akismet.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core.Scoping;

namespace Akismet.Umbraco
{
    public class AkismetService
    {
        private readonly IScopeProvider scopeProvider;

        public AkismetService(IScopeProvider provider)
        {
            scopeProvider = provider;
        }

        internal Dictionary<string, string> GetConfig()
        {
            string appData = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Plugins/akismet");
            if (!File.Exists(Path.Combine(appData, "akismetConfig.json")))
                return new Dictionary<string, string> { { "key", "" }, { "blogUrl", "" } };
            Dictionary<string, string> config = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(appData, "akismetConfig.json")));
            if (config == null)
                return new Dictionary<string, string> { { "key", "" }, { "blogUrl", "" } };

            return config;
        }

        internal void SetConfig(string key, string blogUrl)
        {
            string appData = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Plugins/akismet");
            var config = new Dictionary<string, string> { { "key", key }, { "blogUrl", blogUrl } };
            File.WriteAllText(Path.Combine(appData, "akismetConfig.json"), JsonConvert.SerializeObject(config));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comment">Comment object to be checked</param>
        /// <param name="useStrict">Set to True to only send designated Ham, false to include Unspecified</param>
        /// <returns></returns>
        public bool CheckComment(AkismetComment comment, bool useStrict = false)
        {
            string key, blogUrl;
            var config = GetConfig();
            key = config["key"];
            blogUrl = config["blogUrl"];
            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(blogUrl))
                return true;

            var client = new AkismetClient(key, new Uri(blogUrl), "Umbraco CMS");
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
    }
}