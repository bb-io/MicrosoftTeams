using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.MicrosoftTeams.Dtos;

public class FileDto
{
    public FileDto(FileReference file)
    {
        File = file;
    }
    
    public FileReference File { get; set; }
}