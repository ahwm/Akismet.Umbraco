# Akismet for Umbraco 17

This package provides Akismet spam detection functionality for Umbraco 17 CMS using the **AkismetApi.Net** library.

## Features

- Database migrations for storing Akismet submissions
- AkismetService for checking comments against Akismet API
- API endpoints for managing spam comments
- Configuration-based setup
- Full statistics support from Akismet API

## Configuration

Add the following to your `appsettings.json`:

```json
{
  "Akismet": {
    "ApiKey": "your-akismet-api-key",
    "BlogUrl": "https://yourdomain.com"
  }
}
```

## Database Migrations

The package automatically creates the following database tables on startup:

- **AkismetSubmission**: Stores all submissions checked by Akismet

The migrations will run automatically when the application starts.

## API Endpoints

All endpoints are available under `/umbraco/akismetumbraco/api/v1/`:

- `GET ping` - Test endpoint
- `GET verify-key` - Verify your Akismet API key
- `GET stats` - Get spam statistics from Akismet
- `GET comments` - Get all comments
- `GET spam` - Get spam comments only
- `GET comment/{id}` - Get a specific comment
- `GET spam-count` - Get total spam count
- `GET ham-count` - Get total ham (non-spam) count
- `POST check` - Check if a comment is spam
- `DELETE comment/{id}` - Delete comment(s)
- `POST report-ham/{id}` - Report false positive to Akismet
- `POST report-spam/{id}` - Report missed spam to Akismet

## Usage Example

```csharp
// Inject AkismetService into your controller or service
public class MyController : Controller
{
    private readonly AkismetService _akismetService;
    
    public MyController(AkismetService akismetService)
    {
        _akismetService = akismetService;
    }
    
    public async Task<IActionResult> CheckComment(string content, string author, string email)
    {
        var comment = new AkismetComment
        {
            BlogUrl = "https://yourdomain.com",
            CommentContent = content,
            CommentAuthor = author,
            CommentAuthorEmail = email,
            CommentType = AkismentCommentType.Comment,
            UserIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers["User-Agent"]
        };
        
        var result = await _akismetService.CheckCommentAsync(comment);
        
        if (result.SpamStatus == SpamStatus.Spam)
        {
            // Handle spam
            _akismetService.SaveComment(comment, "comment", "spam");
        }
        
        return Ok();
    }
}
```

## NuGet Package

This package uses **AkismetApi.Net** (version 4.1.0) which is a modern, actively maintained .NET library for the Akismet API.

## Migration from Umbraco 13

This package has been migrated from Umbraco 13 to Umbraco 17 with the following changes:

- Updated to use Umbraco 17 APIs
- Switched from the old Akismet package to **AkismetApi.Net** for better .NET compatibility
- Updated database migration patterns
- Updated notification handlers
- Modernized SQL query syntax
- Updated to .NET 10
- Improved dependency injection with proper HttpClient factory usage

All functionality from the Umbraco 13 version has been preserved and enhanced.

## Features Provided by AkismetApi.Net

- ✅ Comment spam checking
- ✅ Submit spam and ham (false positives)
- ✅ Key verification
- ✅ Account statistics
- ✅ Full async/await support
- ✅ Modern .NET Standard 2.0 compatibility
- ✅ Proper HttpClient integration
