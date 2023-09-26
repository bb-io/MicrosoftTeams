using File = Blackbird.Applications.Sdk.Common.Files.File;

namespace Apps.MicrosoftTeams.Dtos;

public class FileDto
{
    public FileDto(File file)
    {
        File = file;
    }
    
    public File File { get; set; }
}