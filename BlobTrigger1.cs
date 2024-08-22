using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Company.Function
{
    public class BlobTrigger1
    {
        private readonly ILogger<BlobTrigger1> _logger;

        public BlobTrigger1(ILogger<BlobTrigger1> logger)
        {
            _logger = logger;
        }

        // Trigger a Storage account blob when a file arrives, process it, and send it back to another blob
        [Function(nameof(BlobTrigger1))]
        [BlobOutput("test-samples-output/output-{name}")]
        public async Task<byte[]> Run([BlobTrigger("samples-workitems/{name}", Connection = "AzureWebJobsStorage")] byte[] blobContent, string name)
        {
            _logger.LogInformation($"Processing image: {name}");

            using var inputStream = new MemoryStream(blobContent);
            using var outputStream = new MemoryStream();

            try
            {
                using (var image = Image.Load(inputStream))
                {
                    _logger.LogInformation($"Original image size: {image.Width}x{image.Height}");

                    image.Mutate(x => x.Resize(100, 100));
                    _logger.LogInformation($"Resized image size: {image.Width}x{image.Height}");

                    image.SaveAsPng(outputStream);
                }

                _logger.LogInformation($"Image {name} resized and saved to output container.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing image {name}: {ex.Message}");
                throw;
            }

            return outputStream.ToArray();
        }
    }
}