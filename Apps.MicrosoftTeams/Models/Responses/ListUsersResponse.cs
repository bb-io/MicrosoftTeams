using Apps.MicrosoftTeams.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.MicrosoftTeams.Models.Responses
{
    public class ListUsersResponse
    {
        public IEnumerable<UserDto> Users { get; set; }
    }
}
