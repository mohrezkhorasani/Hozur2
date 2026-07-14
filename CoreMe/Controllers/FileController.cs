// Controller برای پشتیبانی از آپلود تکه‌تکه
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly string _tempPath = @"C:\hekmatFiles\temp";
    private readonly string _finalPath = @"C:\hekmatFiles\uploads";

    [HttpPost("UploadChunk")]
    public async Task<IActionResult> UploadChunk(IFormFile file, string fileId, int chunkIndex, int totalChunks, string fileName, string uploadPath)
    {
        var chunkFolder = Path.Combine(_tempPath, fileId);
        if (!Directory.Exists(chunkFolder))
            Directory.CreateDirectory(chunkFolder);

        var chunkPath = Path.Combine(chunkFolder, $"{chunkIndex}");

        using (var stream = new FileStream(chunkPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new { chunkIndex, received = true });
    }

    [HttpPost("MergeChunks")]
    public async Task<IActionResult> MergeChunks([FromBody] MergeRequest request)
    {
        var chunkFolder = Path.Combine(_tempPath, request.FileId);
        var finalFilePath = Path.Combine(_finalPath, request.UploadPath, request.FileName);

        using (var finalStream = new FileStream(finalFilePath, FileMode.Create))
        {
            for (int i = 0; i < request.TotalChunks; i++)
            {
                var chunkPath = Path.Combine(chunkFolder, i.ToString());
                using (var chunkStream = new FileStream(chunkPath, FileMode.Open))
                {
                    await chunkStream.CopyToAsync(finalStream);
                }
                System.IO.File.Delete(chunkPath);
            }
        }

        Directory.Delete(chunkFolder);

        return Ok(new { url = $"/uploads/{request.UploadPath}/{request.FileName}" });
    }
}

public class MergeRequest
{
    public string FileId { get; set; }
    public string FileName { get; set; }
    public int TotalChunks { get; set; }
    public string UploadPath { get; set; }
}