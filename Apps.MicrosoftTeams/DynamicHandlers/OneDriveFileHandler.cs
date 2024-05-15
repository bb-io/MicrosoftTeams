using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.Graph.Models;

namespace Apps.MicrosoftTeams.DynamicHandlers;

public class OneDriveFileHandler(InvocationContext invocationContext)
    : BaseInvocable(invocationContext), IAsyncDataSourceHandler
{
    public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var client = new MSTeamsClient(InvocationContext.AuthenticationCredentialsProviders);
        var drive = await client.Me.Drive.GetAsync(cancellationToken: cancellationToken);
        var filesDictionary = new Dictionary<string, string>();
        var filesAmount = 0;
        string? skipToken;
        var requestInformation = client.Drives[drive.Id].List.Items.ToGetRequestInformation(requestConfiguration =>
        {
            requestConfiguration.Headers.Add("Prefer", "HonorNonIndexedQueriesWarningMayFailRandomly");
            requestConfiguration.QueryParameters.Select = new[] { "id" };
            requestConfiguration.QueryParameters.Expand = new[] { "driveItem" };
            requestConfiguration.QueryParameters.Filter = "fields/ContentType eq 'Document'";
            requestConfiguration.QueryParameters.Top = 2;
        });
        requestInformation.UrlTemplate = requestInformation.UrlTemplate.Insert(requestInformation.UrlTemplate.Length - 1, ",%24skiptoken");

        do
        {
            var files = await client.RequestAdapter.SendAsync(requestInformation,
                ListItemCollectionResponse.CreateFromDiscriminatorValue, cancellationToken: cancellationToken);
            var filteredFiles = files.Value
                .Select(item => item.DriveItem)
                .Select(item => new { item.Id, Path = GetFilePath(item) })
                .Where(item => item.Path.Contains(context.SearchString ?? "", StringComparison.OrdinalIgnoreCase));
            
            foreach (var file in filteredFiles)
                filesDictionary.Add(file.Id, file.Path);
            
            filesAmount += filteredFiles.Count();
            skipToken = files.OdataNextLink?.Split("skiptoken=")[^1];
            requestInformation.QueryParameters["%24skiptoken"] = skipToken;
        } while (filesAmount < 20 && skipToken != null);
        
        foreach (var file in filesDictionary)
        {
            var filePath = file.Value;
            if (filePath.Length > 40)
            {
                var filePathParts = filePath.Split("/");
                if (filePathParts.Length > 3)
                {
                    filePath = string.Join("/", filePathParts[0], "...", filePathParts[^2], filePathParts[^1]);
                    filesDictionary[file.Key] = filePath;
                }
            }
        }

        return filesDictionary;
    }

    private string GetFilePath(DriveItem file)
    {
        var parentPath = file.ParentReference.Path.Split("root:");
        if (parentPath[1] == "")
            return file.Name;

        return $"{parentPath[1].Substring(1)}/{file.Name}";
    }
}