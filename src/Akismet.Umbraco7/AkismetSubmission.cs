using System;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Akismet.Umbraco
{
    [TableName("AkismetSubmission")]
    [PrimaryKey("Id", autoIncrement = true)]
    [ExplicitColumns]
    public class AkismetSubmission
    {
        [PrimaryKeyColumn(AutoIncrement = true, IdentitySeed = 1)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("CommentDate")]
        public DateTime CommentDate { get; set; }

        [Column("CommentType")]
        public string CommentType { get; set; }

        [Column("CommentText")]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        public string CommentText { get; set; }

        [Column("CommentData")]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        public string CommentData { get; set; }

        [Column("Result")]
        public string Result { get; set; }

        [Column("SpamStatus")]
        public int SpamStatus { get; set; }

        [Column("UserIp")]
        public string UserIp { get; set; }

        [Column("UserName")]
        public string UserName { get; set; }
    }
}