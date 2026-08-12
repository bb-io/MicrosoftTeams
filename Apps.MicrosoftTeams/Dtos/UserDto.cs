using Blackbird.Applications.Sdk.Common;
using Microsoft.Graph.Models;
using System.Net;

namespace Apps.MicrosoftTeams.Dtos;

public class UserDto
{
    public UserDto(User user) 
    {
        BusinessPhones = user.BusinessPhones;
        DisplayName = user.DisplayName;
        GivenName = user.GivenName;
        JobTitle = user.JobTitle;
        Mail = user.Mail;
        MobilePhone = user.MobilePhone;
        OfficeLocation = user.OfficeLocation;
        PreferredLanguage = user.PreferredLanguage;
        Surname = user.Surname;
        UserPrincipalName = user.UserPrincipalName;
        Id = user.Id;
    }

    [Display("User ID")]
    public string Id { get; set; }

    [Display("Mention user")]
    public string MentionUser => string.IsNullOrWhiteSpace(Id)
        ? string.Empty
        : $"<at user_id=\"{WebUtility.HtmlEncode(Id)}\">{WebUtility.HtmlEncode(DisplayName)}</at>";

    public List<string> BusinessPhones { get; set; }
    public string DisplayName { get; set; }
    public string GivenName { get; set; }
    public string JobTitle { get; set; }
    public string Mail { get; set; }
    public string MobilePhone { get; set; }
    public string OfficeLocation { get; set; }
    public string PreferredLanguage { get; set; }
    public string Surname { get; set; }
    public string UserPrincipalName { get; set; }
}
