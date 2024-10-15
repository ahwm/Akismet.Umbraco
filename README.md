# Akismet Management for Umbraco 8
Adds Akismet dashboard to Umbraco backoffice.

![image](https://user-images.githubusercontent.com/20478373/128535412-0b87914f-a848-4365-a631-ff8ffd92d448.png)

*Note: This package for Umbraco 8 **only**. Check [here](https://github.com/ahwm/Akismet.Umbraco/tree/umbraco9) for Umbraco 9*

## Installation

### Install via NuGet:

```
Install-Package Akismet.Umbraco8
```

[![NuGet Status](https://img.shields.io/nuget/vpre/Akismet.Umbraco8)](https://www.nuget.org/packages/Akismet.Umbraco8/)
![NuGet Downloads](https://img.shields.io/nuget/dt/Akismet.Umbraco8)


### Post-Installation

After package installation, log into the backoffice and navigate to Users, Groups, Administrators and be sure to add Akismet.

![image](https://user-images.githubusercontent.com/20478373/112378469-9828b400-8cac-11eb-8a9e-8d35155aab0e.png)

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
