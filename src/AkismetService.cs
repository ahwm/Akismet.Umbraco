using Akismet.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Akismet.Umbraco
{
    public static class ServicesConfiguration
    {
        public static IUmbracoBuilder AddAkismet(this IUmbracoBuilder builder)
        {
            builder.Services.Configure<AkismetOptions>(builder.Config.GetSection(AkismetOptions.SectionName));

            // Get configuration to register Akismet client
            var akismetOptions = new AkismetOptions();
            builder.Config.GetSection(AkismetOptions.SectionName).Bind(akismetOptions);

            if (!string.IsNullOrEmpty(akismetOptions.ApiKey))
            {
                builder.Services.AddAkismet(akismetOptions.ApiKey, "Umbraco CMS");
            }

            // Register the service
            builder.Services.AddTransient<AkismetService>();

            return builder;
        }
    }

    public class AkismetService(IScopeProvider scopeProvider, AkismetClient client)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="blogUrl"></param>
        /// <returns></returns>
        public async Task<bool> VerifyKeyAsync(string blogUrl)
        {
            if (String.IsNullOrWhiteSpace(blogUrl))
                return false;

            bool isValid = await client.VerifyKeyAsync(blogUrl);

            return isValid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<AkismetAccount> GetAccountAsync(string blogUrl)
        {
            if (String.IsNullOrWhiteSpace(blogUrl))
                return new AkismetAccount();

            return await client.GetAccountStatusAsync(blogUrl);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="comment">Comment object to be checked</param>
        /// <param name="useStrict">Set to True to only send designated Ham, false to include Unspecified</param>
        /// <returns></returns>
        public async Task<bool> CheckCommentAsync(AkismetComment comment, bool useStrict = false)
        {
            if (String.IsNullOrWhiteSpace(comment.BlogUrl))
                return true;

            var result = await client.CheckAsync(comment);

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
            List<int> ids = [.. id.Split(',').Select(x => Convert.ToInt32(x))];
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
        public async Task ReportHamAsync(string id)
        {
            List<int> ids = [.. id.Split(',').Select(x => Convert.ToInt32(x))];
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("AkismetSubmission").Where<AkismetSubmission>(x => ids.Contains(x.Id));

                var comments = scope.Database.Query<AkismetSubmission>(sql).ToList();

                foreach (var comment in comments)
                {
                    AkismetComment c = JsonConvert.DeserializeObject<AkismetComment>(comment.CommentData);
                    c.CommentDate = comment.CommentDate.ToString("s");
                    c.CommentType = comment.CommentType;
                    await client.SubmitHamAsync(c);
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
        public async Task ReportSpamAsync(string id)
        {
            List<int> ids = [.. id.Split(',').Select(x => Convert.ToInt32(x))];
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("AkismetSubmission").Where<AkismetSubmission>(x => ids.Contains(x.Id));

                var comments = scope.Database.Query<AkismetSubmission>(sql).ToList();

                foreach (var comment in comments)
                {
                    AkismetComment c = JsonConvert.DeserializeObject<AkismetComment>(comment.CommentData);
                    c.CommentDate = comment.CommentDate.ToString("s");
                    c.CommentType = comment.CommentType;
                    await client.SubmitSpamAsync(c);
                }
            }
            DeleteComment(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<dynamic> GetStatsAsync(string blogUrl)
        {
            if (String.IsNullOrWhiteSpace(blogUrl))
                return null;

            var respAll = await client.GetStatisticsAsync("all");
            var resp = await client.GetStatisticsAsync(blogUrl);
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