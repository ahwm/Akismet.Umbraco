using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Akismet.Umbraco.Models
{
    [TableName("AkismetSubmission")]
    [PrimaryKey("Id", AutoIncrement = true)]
    [ExplicitColumns]
    public class AkismetSubmission
    {
        [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("CommentDate")]
        public DateTime CommentDate { get; set; }

        [Column("CommentType")]
        public string CommentType { get; set; } = string.Empty;

        [Column("CommentText")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string CommentText { get; set; } = string.Empty;

        [Column("CommentData")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string CommentData { get; set; } = string.Empty;

        [Column("Result")]
        public string Result { get; set; } = string.Empty;

        [Column("SpamStatus")]
        public int SpamStatus { get; set; }

        [Column("UserIp")]
        public string? UserIp { get; set; }

        [Column("UserName")]
        public string? UserName { get; set; }
    }
}
