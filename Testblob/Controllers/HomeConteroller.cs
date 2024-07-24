using Microsoft.AspNetCore.Mvc;

namespace Testblob.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeConteroller : ControllerBase
    {
        private readonly FileService _fileService;

        public HomeConteroller(FileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _fileService.GetAllAsync();
            return Ok(result);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is not selected.");

            var blobId = Guid.NewGuid();
            var fileName = file.FileName;
            var contentType = file.ContentType;

            using (var stream = file.OpenReadStream())
            {
                await _fileService.UploadBlobAsync(blobId, fileName, stream, contentType);
            }

            return Ok(new { Id = blobId, FileName = fileName });
        }

        [HttpGet]
        [Route("download/{id:guid}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var result = await _fileService.DownloadAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            return File(result.Content, result.ContentType, result.Name);
        }

    }
}
