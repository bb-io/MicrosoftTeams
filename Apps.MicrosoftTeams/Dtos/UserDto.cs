using Blackbird.Applications.Sdk.Common;
using Microsoft.Graph.Models;

namespace Apps.MicrosoftTeams.Dtos
{
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
}
