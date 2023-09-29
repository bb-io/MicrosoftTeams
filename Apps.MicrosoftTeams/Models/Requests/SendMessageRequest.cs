using Apps.MicrosoftTeams.DynamicHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.MicrosoftTeams.Models.Requests;

public class SendMessageRequest
{
    public string Message { get; set; }
    
    [Display("Attachment file")]
    public File? AttachmentFile { get; set; }
    
    [Display("Attachment file from OneDrive")]
    [DataSource(typeof(OneDriveFileHandler))]
    public string? OneDriveAttachmentFileId { get; set; }
}