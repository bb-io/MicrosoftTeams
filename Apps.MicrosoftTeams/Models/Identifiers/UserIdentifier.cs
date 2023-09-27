﻿using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.MicrosoftTeams.Models.Identifiers;

public class UserIdentifier
{
    [Display("User")]
    [DataSource(typeof(UserHandler))]
    public string UserId { get; set; }
}