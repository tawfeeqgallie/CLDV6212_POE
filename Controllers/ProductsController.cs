// REFERENCES:
// https://learn.microsoft.com/aspnet/core/fundamentals/startup
// https://learn.microsoft.com/azure/storage/blobs/storage-quickstart-blobs-dotnet
// https://learn.microsoft.com/azure/storage/tables/table-storage-overview

using Azure.Data.Tables;
using CLDV6212_POE.Data;
using CLDV6212_POE.Models;
using CLDV6212_POE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLDV6212_POE.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AzureBlobService _blobService;
        private readonly AzureTableService _tableService;
        private readonly InMemoryRepository _repo;
        private readonly IConfiguration _config;
        private readonly AppDbContext _db; // SQL injection added

        public ProductsController(
            AzureBlobService blobService,
            AzureTableService tableService,
            InMemoryRepository repo,
            IConfiguration config,
            AppDbContext db)
        {
            _blobService = blobService;
            _tableService = tableService;
            _repo = repo;
            _config = config;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _db.Products.ToListAsync();
            return View(products);
        }

        // GET: CREATE PRODUCT FORM
        [HttpGet]
        public IActionResult Create() => View();

        // POST: CREATE PRODUCT
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(Product model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (imageFile != null && imageFile.Length > 0)
            {
                var blobSvc = _blobService.GetClient();
                if (blobSvc != null)
                {
                    var containerName = _config["BlobContainer"] ?? "product-images";
                    var container = blobSvc.GetBlobContainerClient(containerName);

                    await container.CreateIfNotExistsAsync(
                        Azure.Storage.Blobs.Models.PublicAccessType.Blob);

                    var blob = container.GetBlobClient($"{Guid.NewGuid():n}-{imageFile.FileName}");

                    await using var stream = imageFile.OpenReadStream();
                    await blob.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
                    {
                        ContentType = imageFile.ContentType
                    });

                    model.ImageUrl = blob.Uri.ToString();
                }
            }

            _db.Products.Add(model);
            await _db.SaveChangesAsync();

            var tableSvc = _tableService.GetClient();
            if (tableSvc != null)
            {
                var tableName = _config["ProductsTable"] ?? "Products";
                var tableClient = tableSvc.GetTableClient(tableName);
                await tableClient.CreateIfNotExistsAsync();

                var entity = new TableEntity("Products", model.Id)
                {
                    { "Name", model.Name },
                    { "Price", model.Price },
                    { "Quantity", model.Quantity },
                    { "ImageUrl", model.ImageUrl ?? string.Empty }
                };

                await tableClient.AddEntityAsync(entity);
            }

            _repo.Products.Add(model);

            TempData["ok"] = "Product created successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(string id)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}
