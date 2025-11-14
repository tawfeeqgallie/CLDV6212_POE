using CLDV6212_POE.Models;
using CLDV6212_POE.Services;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Files.Shares;
using System.Net.Mime;

namespace CLDV6212_POE.Controllers
{
    public class FilesController : Controller
    {
        private readonly AzureFileService _fileService;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public FilesController(AzureFileService fileService, IWebHostEnvironment env, IConfiguration config)
        {
            _fileService = fileService;
            _env = env;
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            var items = await ListFilesAsync();
            return View(items);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select a file");
                var items = await ListFilesAsync();
                return View("Index", items);
            }

            var shareSvc = _fileService.GetClient();
            var fileName = $"{Guid.NewGuid():n}-{file.FileName}";
            var shareName = _config["FilesShare"] ?? "contracts";

            if (shareSvc != null)
            {
                var share = shareSvc.GetShareClient(shareName);
                await share.CreateIfNotExistsAsync();
                var dir = share.GetRootDirectoryClient();
                var cFile = dir.GetFileClient(fileName);
                await cFile.CreateAsync(file.Length);
                await using var s = file.OpenReadStream();
                await cFile.UploadAsync(s);
            }
            else
            {
                var dir = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads", shareName);
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, fileName);
                await using var fs = System.IO.File.Create(path);
                await file.CopyToAsync(fs);
            }

            TempData["ok"] = "File uploaded";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<FileItemVm>> ListFilesAsync()
        {
            var list = new List<FileItemVm>();
            var shareSvc = _fileService.GetClient();
            var shareName = _config["FilesShare"] ?? "contracts";

            if (shareSvc != null)
            {
                var share = shareSvc.GetShareClient(shareName);
                await share.CreateIfNotExistsAsync();
                var dir = share.GetRootDirectoryClient();

                await foreach (var item in dir.GetFilesAndDirectoriesAsync())
                {
                    if (item.IsDirectory) continue;

                    list.Add(new FileItemVm
                    {
                        Name = item.Name,
                        Length = item.FileSize ?? 0,
                        LastModified = item.Properties.LastModified,
                        // link to our own action
                        Url = Url.Action("Open", "Files", new { fileName = item.Name })
                    });
                }
            }
            else
            {
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var dirPath = Path.Combine(root, "uploads", shareName);
                if (Directory.Exists(dirPath))
                {
                    foreach (var path in Directory.GetFiles(dirPath))
                    {
                        var fi = new FileInfo(path);
                        list.Add(new FileItemVm
                        {
                            Name = fi.Name,
                            Length = fi.Length,
                            LastModified = new DateTimeOffset(fi.LastWriteTimeUtc, TimeSpan.Zero),
                            Url = $"/uploads/{shareName}/{fi.Name}"
                        });
                    }
                }
            }

            return list.OrderByDescending(x => x.LastModified).ToList();
        }

        public async Task<IActionResult> Open(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return NotFound();

            var shareSvc = _fileService.GetClient();
            var shareName = _config["FilesShare"] ?? "contracts";

            if (shareSvc != null)
            {
                var share = shareSvc.GetShareClient(shareName);
                var dir = share.GetRootDirectoryClient();
                var fileClient = dir.GetFileClient(fileName);

                if (!await fileClient.ExistsAsync())
                    return NotFound();

                var download = await fileClient.DownloadAsync();
                var stream = download.Value.Content;

                // try guess content type
                var contentType = "application/octet-stream";
                if (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/jpeg";
                else if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/png";
                else if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    contentType = "application/pdf";

                return File(stream, contentType, fileName); // inline
            }
            else
            {
                // fallback to local
                var root = _env.WebRootPath ?? _env.ContentRootPath;
                var path = Path.Combine(root, "uploads", shareName, fileName);
                if (!System.IO.File.Exists(path))
                    return NotFound();

                var contentType = "application/octet-stream";
                return PhysicalFile(path, contentType, fileName);
            }
        }
    }
}
