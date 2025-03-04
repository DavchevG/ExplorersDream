using ExplorersDream.Data;
using ExplorersDream.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ExplorersDream.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Метод за списък с продукти
            public async Task<IActionResult> Index(int? category, decimal? minPrice, decimal? maxPrice, string searchQuery, Product p)
            {
                var productsQuery = _dbContext.Products
                    .Include(p => p.Images)
                    .AsQueryable();

                // Филтриране по категория
                if (category.HasValue && category.Value > 0)
                {
                    productsQuery = productsQuery.Where(p => p.CategoryID == category.Value);
                }

                // Филтриране по минимална цена
                if (minPrice.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
                }

                // Филтриране по максимална цена
                if (maxPrice.HasValue)
                {
                    productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
                }

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    productsQuery = productsQuery.Where( p => p.Name.Contains(searchQuery));
                }


                var products = await productsQuery.ToListAsync();
                var isAdmin = User.IsInRole("Admin");
                ViewBag.IsAdmin = isAdmin;

                // Зареждане на категориите за dropdown в изгледа
                ViewBag.Categories = await _dbContext.Categories
                    .Select(c => new { c.Id, c.Name })
                    .ToListAsync();

                // Запазване на избраните филтри
                ViewBag.SelectedCategory = category;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                ViewBag.SearchQuery = searchQuery;

            return View(products);
            }



        // Метод за детайли на продукт
        public async Task<IActionResult> Details(int id)
        {
            var product = await _dbContext.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // Показване на формата за добавяне
        public IActionResult Create()
        {
            // Зареждане на категориите
            var categories = _dbContext.Categories.Select(c => new { c.Id, c.Name }).ToList();

            // Проверка дали има налични категории
            if (categories == null || !categories.Any())
            {
                ViewBag.Categories = new List<SelectListItem>(); // Празен списък, за да се избегне грешка
            }
            else
            {
                ViewBag.Categories = categories;
            }

            return View();
        }


        // Метод за добавяне на нов продукт
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                // Качване на изображения
                product.Images = await UploadImages(imageFiles);

                // Добавяне на продукта в базата
                _dbContext.Products.Add(product);
                await _dbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "Продуктът беше добавен успешно!";
                return RedirectToAction(nameof(Index));
            }

            // Ако валидацията не мине, зареди категориите отново
            ViewBag.Categories = await _dbContext.Categories
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return View(product);
        }


        // Показване на формата за редактиране
        public IActionResult Edit(int id)
        {
            var product = _dbContext.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            // Зареждане на категориите, ако ги използваш
            ViewBag.Categories = _dbContext.Categories.ToList();

            return View(product);
        }

        // Запазване на промените
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product updatedProduct, List<IFormFile> imageFile)
        {
            if (id != updatedProduct.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var product = await _dbContext.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
                if (product == null)
                {
                    return NotFound();
                }

                // Актуализиране на свойствата на продукта
                product.Name = updatedProduct.Name;
                product.ShortDescription = updatedProduct.ShortDescription;
                product.Description = updatedProduct.Description;
                product.Price = updatedProduct.Price;
                product.CategoryID = updatedProduct.CategoryID;

                // Обработка на нови изображения (ако има)
                if (imageFile != null && imageFile.Any())
                {
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    Directory.CreateDirectory(uploadPath); // Създава директорията, ако не съществува

                    // Изтриване на старите изображения
                    if (product.Images.Any())
                    {
                        foreach (var oldImage in product.Images)
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImage.Url.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath); // Изтриване на файла
                            }
                        }

                        _dbContext.ProductImages.RemoveRange(product.Images); // Изтриване на записите в базата данни
                    }

                    // Добавяне на новите изображения
                    var newImages = new List<ProductImage>();
                    foreach (var file in imageFile)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}"; // Генериране на уникално име
                        var filePath = Path.Combine(uploadPath, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        newImages.Add(new ProductImage
                        {
                            Url = $"/images/{uniqueFileName}"
                        });
                    }

                    product.Images = newImages;
                }

                _dbContext.Products.Update(product);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Зареждане на категориите отново при грешка
            ViewBag.Categories = _dbContext.Categories.ToList();


            return View(updatedProduct);
        }



        // Метод за изтриване на продукт
        public IActionResult Delete(int id)
        {
            var product = _dbContext.Products
                .Include(p => p.Images)
                .FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _dbContext.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Изтриване на изображения
            foreach (var image in product.Images)
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.Url.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        // Помощен метод за качване на изображения
        private async Task<List<ProductImage>> UploadImages(List<IFormFile> imageFiles)
        {
            var images = new List<ProductImage>();
            var uploadPath = GetUploadPath();

            foreach (var file in imageFiles)
            {
                var fileName = Path.GetFileName(file.FileName);
                var fileExtension = Path.GetExtension(fileName).ToLower();

                // Валидация на типа на файла
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ImageFiles", "Позволени са само JPG, JPEG, PNG и GIF файлове.");
                    continue;
                }

                // Ограничение за размер
                if (file.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFiles", $"Файлът {fileName} е твърде голям. Максималният размер е 5 MB.");
                    continue;
                }

                var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                images.Add(new ProductImage
                {
                    Url = $"/images/{uniqueFileName}"
                });
            }

            return images;
        }

        // Помощен метод за изтриване на изображения
        private async Task DeleteImages(IEnumerable<ProductImage> images)
        {
            foreach (var image in images)
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.Url.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                _dbContext.ProductImages.Remove(image);
            }

            await _dbContext.SaveChangesAsync();
        }

        // Помощен метод за получаване на директория за качване
        private string GetUploadPath()
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            Directory.CreateDirectory(uploadPath); // Създава директорията, ако не съществува
            return uploadPath;
        }
    }
}
