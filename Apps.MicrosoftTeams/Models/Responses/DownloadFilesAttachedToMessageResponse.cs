using Apps.MicrosoftTeams.Dtos;

namespace Apps.MicrosoftTeams.Models.Responses;

public class DownloadFilesAttachedToMessageResponse
{
    public IEnumerable<FileDto> Files { get; set; }
}