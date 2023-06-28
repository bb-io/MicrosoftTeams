using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string Id { get; set; }
    }
}
