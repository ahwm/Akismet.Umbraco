using Akismet.Net;
using Akismet.Umbraco.Models;
using Akismet.Umbraco.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Akismet.Umbraco.Controllers
{
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "Akismet.Umbraco")]
    public class AkismetUmbracoApiController(AkismetService akismetService) : AkismetUmbracoApiControllerBase
    {
        private readonly AkismetService _akismetService = akismetService;

        [HttpGet("verify-key")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        public async Task<IActionResult> VerifyKey()
        {
            if (!_akismetService.IsConfigured)
                return BadRequest("Akismet is not configured");

            var result = await _akismetService.VerifyKeyAsync();
            return Ok(result);
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(SpamStats), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStats()
        {
            if (!_akismetService.IsConfigured)
                return BadRequest("Akismet is not configured");

            var stats = await _akismetService.GetStatsAsync();
            return Ok(stats);
        }

        [HttpGet("comments")]
        [ProducesResponseType<IEnumerable<AkismetSubmission>>(StatusCodes.Status200OK)]
        public IActionResult GetHamComments()
        {
            var comments = _akismetService.GetHamComments();
            return Ok(comments);
        }

        [HttpGet("spam")]
        [ProducesResponseType<IEnumerable<AkismetSubmission>>(StatusCodes.Status200OK)]
        public IActionResult GetSpamComments()
        {
            var comments = _akismetService.GetSpamComments();
            return Ok(comments);
        }

        [HttpGet("comment/{id}")]
        [ProducesResponseType<AkismetSubmission>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetComment(int id)
        {
            var comment = _akismetService.GetComment(id);
            if (comment == null)
                return NotFound();

            return Ok(comment);
        }

        [HttpPost("check")]
        [ProducesResponseType(typeof(AkismetResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckComment([FromBody] AkismetComment comment)
        {
            if (!_akismetService.IsConfigured)
                return BadRequest("Akismet is not configured");

            var result = await _akismetService.CheckCommentAsync(comment);
            return Ok(result);
        }

        [HttpDelete("comments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult DeleteComment(string ids)
        {
            _akismetService.DeleteComment(ids);
            return Ok();
        }

        [HttpPost("report-ham/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReportHam(string id)
        {
            if (!_akismetService.IsConfigured)
                return BadRequest("Akismet is not configured");

            await _akismetService.ReportHamAsync(id);
            return Ok();
        }

        [HttpPost("report-spam/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReportSpam(string id)
        {
            if (!_akismetService.IsConfigured)
                return BadRequest("Akismet is not configured");

            await _akismetService.ReportSpamAsync(id);
            return Ok();
        }

        [HttpGet("spam-count")]
        [ProducesResponseType<int>(StatusCodes.Status200OK)]
        public IActionResult GetSpamCount()
        {
            var count = _akismetService.GetTotalSpamCount();
            return Ok(count);
        }

        [HttpGet("ham-count")]
        [ProducesResponseType<int>(StatusCodes.Status200OK)]
        public IActionResult GetHamCount()
        {
            var count = _akismetService.GetTotalHamCount();
            return Ok(count);
        }
    }
}
