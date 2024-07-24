using System.Globalization;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Testblob
{
    public class FileService
    {
        private readonly string _storageAccount = "masterstream2";
        private readonly string _key = "ympfVWXTk5UCD54658pPL8hfoNYI9F7mlQubqE8oH2BwWGLs3qU+ZiRyjpC0r3mlw3rpOwm3iZbu+AStyw6Vow==";
        private readonly BlobContainerClient filesContainer;

        public FileService()
        {
            var credential = new StorageSharedKeyCredential(_storageAccount, _key);
            var blobUri = $"https://{_storageAccount}.blob.core.windows.net";
            var blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
            filesContainer = blobServiceClient.GetBlobContainerClient("photo");
        }

        public async Task<IEnumerable<BlobDto>> GetAllAsync()
        {
            var blobDtos = new List<BlobDto>();

            await foreach (var blobItem in filesContainer.GetBlobsAsync())
            {
                BlobClient blobClient = filesContainer.GetBlobClient(blobItem.Name);
                BlobProperties properties = await blobClient.GetPropertiesAsync();

                Guid blobId = Guid.Empty;
                if (properties.Metadata.TryGetValue("blobId", out string storedBlobId))
                {
                    Guid.TryParse(storedBlobId, out blobId);
                }

                blobDtos.Add(new BlobDto
                {
                    Id = blobId,
                    Uri = blobClient.Uri.ToString(),
                    Name = blobItem.Name,
                    ContentType = properties.ContentType,
                    // Content = null because we are just listing the blobs, not downloading the content
                });
            }

            return blobDtos;
        }


        public async Task UploadBlobAsync(Guid blobId, string fileName, Stream content, string contentType)
        {
            BlobClient blobClient = filesContainer.GetBlobClient(fileName);

            var metadata = new Dictionary<string, string>
    {
        { "blobId", blobId.ToString() }
    };

            var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };

            await blobClient.UploadAsync(content, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders,
                Metadata = metadata
            });
        }


        public async Task<BlobDto> DownloadAsync(Guid blobId)
        {
            string blobFileName = await GetFileNameByIdAsync(blobId);

            if (blobFileName == null)
            {
                return null;
            }

            BlobClient file = filesContainer.GetBlobClient(blobFileName);

            if (await file.ExistsAsync())
            {
                var data = await file.OpenReadAsync();
                Stream blobContent = data;

                var content = await file.DownloadContentAsync();

                string name = blobFileName;
                string contentType = content.Value.Details.ContentType;
                return new BlobDto { Id = blobId, Content = blobContent, Name = name, ContentType = contentType };
            }

            return null;
        }

        private async Task<string> GetFileNameByIdAsync(Guid blobId)
        {
            await foreach (var blobItem in filesContainer.GetBlobsAsync())
            {
                BlobClient blobClient = filesContainer.GetBlobClient(blobItem.Name);
                BlobProperties properties = await blobClient.GetPropertiesAsync();

                if (properties.Metadata.TryGetValue("blobId", out string storedBlobId) && Guid.TryParse(storedBlobId, out Guid storedGuid))
                {
                    if (storedGuid == blobId)
                    {
                        return blobItem.Name;
                    }
                }
            }

            return null;
        }


    }
}
