# Akismet Management for Umbraco
Adds Akismet dashboard to Umbraco backoffice.

![image](https://user-images.githubusercontent.com/20478373/112371956-ce623580-8ca4-11eb-9c7f-a87e58e42e97.png)

## Installation

Install via NuGet:

```
Install-Package Akismet.Umbraco8 -Version 0.1.0-alpha
```

[![NuGet Status](https://buildstats.info/nuget/Akismet.Umbraco8?includePreReleases=true)](https://www.nuget.org/packages/Akismet.Umbraco8/)

## Usage
Once installed, the `AkismetService` service becomes available for checking comments. It is up to the developer to wire up to the contact forms but here is an abbreviated example:

```csharp
using Akismet.NET;
using Akismet.Umbraco;

public class ContactFormController : SurfaceController
{
    private readonly AkismetService AkismetService;
    
    public ContactFormController(AkismetService akismetService)
    {
        AkismetService = akismetService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult SubmitForm(ContactModel model)
    {
        // check validation
        // eg - required fields, captcha, etc
        
        string ip = Request.Headers["CF-Connecting-IP"] ?? Request.UserHostAddress;
        if (String.IsNullOrWhiteSpace(ip))
            ip = Request.ServerVariables["REMOTE_HOST"];

        // see https://github.com/ahwm/Akismet.Net for all available options
        AkismetComment comment = new AkismetComment
        {
            CommentAuthor = model.Name,
            CommentAuthorEmail = model.EmailAddress,
            Referrer = Request.UrlReferrer.ToString(),
            UserAgent = Request.UserAgent,
            UserIp = ip,
            CommentContent = model.Message,
            CommentType = AkismentCommentType.ContactForm,
            Permalink = CurrentPage.Url(mode: UrlMode.Absolute)
        };
        
        // Specify true on CheckComment() to not include "Unspecified" as valid comments
        bool isValid = AkismetService.CheckComment(comment, true);
        if (!isValid)
        {
            return CurrentUmbracoPage();
        }
        
        // continue processing
    }
}
```
