using Governate_ERP_System.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Governate_ERP_System.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ITenantCatalogService _catalog;

        public ProjectsController(ITenantCatalogService catalog) => _catalog = catalog;

        // GET: /Projects
        public async Task<IActionResult> Index() => View(await _catalog.ListAsync());

        // GET: /Projects/Create
        public IActionResult Create() => View();

        // POST: /Projects/Create
        [HttpPost]
        public async Task<IActionResult> Create(string code, string name)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "الرمز والاسم إجباريان.");
                return View();
            }

            var p = await _catalog.CreateProjectAsync(code, name);
            // الآن تم إنشاء قاعدة بيانات وجميع الجداول الخاصة بالمشروع (تسمع في SQL)
            return RedirectToAction(nameof(Index));
        }
    }
}
