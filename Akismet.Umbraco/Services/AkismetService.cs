using Akismet.Net;
using Akismet.Umbraco.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Akismet.Umbraco.Services
{
    public class AkismetService
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly AkismetClient _akismetClient;
        private readonly IConfiguration _configuration;
        private readonly string? _apiKey;
        private readonly string? _blogUrl;

        public AkismetService(
            IScopeProvider scopeProvider, 
            AkismetClient akismetClient,
            IConfiguration configuration)
        {
            _scopeProvider = scopeProvider;
            _akismetClient = akismetClient;
            _configuration = configuration;
            _apiKey = _configuration["Akismet:ApiKey"];
            _blogUrl = _configuration["Akismet:BlogUrl"];
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey) && !string.IsNullOrWhiteSpace(_blogUrl);

        public async Task<bool> VerifyKeyAsync()
        {
            if (!IsConfigured)
                return false;

            return await _akismetClient.VerifyKeyAsync(_blogUrl!);
        }

        public async Task<AkismetResponse> CheckCommentAsync(AkismetComment comment)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("Akismet is not configured");

            return await _akismetClient.CheckAsync(comment);
        }

        public void SaveComment(AkismetComment comment, string commentType, string result)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            
            var submission = new AkismetSubmission
            {
                CommentDate = DateTime.UtcNow,
                CommentType = commentType,
                CommentText = comment.CommentContent ?? string.Empty,
                CommentData = JsonConvert.SerializeObject(comment),
                Result = result,
                SpamStatus = result == "spam" ? 1 : 0,
                UserIp = comment.UserIp,
                UserName = comment.CommentAuthor
            };

            scope.Database.Insert(submission);
            scope.Complete();
        }

        public IEnumerable<AkismetSubmission> GetAllComments()
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            
            var sql = scope.SqlContext.Sql()
                .Select("*")
                .From("AkismetSubmission")
                .OrderByDescending("CommentDate");

            return scope.Database.Query<AkismetSubmission>(sql);
        }

        public IEnumerable<AkismetSubmission> GetSpamComments()
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            
            var sql = scope.SqlContext.Sql()
                .Select("*")
                .From("AkismetSubmission")
                .Where("SpamStatus = 1")
                .OrderByDescending("CommentDate");

            return scope.Database.Query<AkismetSubmission>(sql);
        }

        public AkismetSubmission? GetComment(int id)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            
            var sql = scope.SqlContext.Sql()
                .Select("*")
                .From("AkismetSubmission")
                .Where("Id = @0", id);

            return scope.Database.FirstOrDefault<AkismetSubmission>(sql);
        }

        public void DeleteComment(string id)
        {
            var ids = id.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            
            foreach (var commentId in ids)
            {
                var sql = scope.SqlContext.Sql()
                    .Append("DELETE FROM AkismetSubmission WHERE Id = @0", commentId);

                scope.Database.Execute(sql);
            }
            
            scope.Complete();
        }

        public async Task ReportHamAsync(string id)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("Akismet is not configured");

            var ids = id.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            
            foreach (var commentId in ids)
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*")
                    .From("AkismetSubmission")
                    .Where("Id = @0", commentId);

                var comment = scope.Database.FirstOrDefault<AkismetSubmission>(sql);
                
                if (comment != null)
                {
                    var akismetComment = JsonConvert.DeserializeObject<AkismetComment>(comment.CommentData);
                    
                    if (akismetComment != null)
                    {
                        await _akismetClient.SubmitHamAsync(akismetComment);
                    }
                }
            }

            scope.Complete();
            DeleteComment(id);
        }

        public async Task ReportSpamAsync(string id)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("Akismet is not configured");

            var ids = id.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            
            foreach (var commentId in ids)
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*")
                    .From("AkismetSubmission")
                    .Where("Id = @0", commentId);

                var comment = scope.Database.FirstOrDefault<AkismetSubmission>(sql);
                
                if (comment != null)
                {
                    var akismetComment = JsonConvert.DeserializeObject<AkismetComment>(comment.CommentData);
                    
                    if (akismetComment != null)
                    {
                        await _akismetClient.SubmitSpamAsync(akismetComment);
                    }
                }
            }

            scope.Complete();
            DeleteComment(id);
        }

        public async Task<SpamStats?> GetStatsAsync()
        {
            if (!IsConfigured)
                return null;

            return await _akismetClient.GetStatisticsAsync(_blogUrl!, _apiKey!);
        }

        public int GetTotalSpamCount()
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            
            var sql = scope.SqlContext.Sql()
                .Select("COUNT(*)")
                .From("AkismetSubmission")
                .Where("SpamStatus = 1");

            return scope.Database.ExecuteScalar<int>(sql);
        }

        public int GetTotalHamCount()
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            
            var sql = scope.SqlContext.Sql()
                .Select("COUNT(*)")
                .From("AkismetSubmission")
                .Where("SpamStatus = 0");

            return scope.Database.ExecuteScalar<int>(sql);
        }
    }
}
