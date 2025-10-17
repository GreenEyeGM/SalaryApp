using Microsoft.AspNetCore.Mvc;

namespace SalaryApp.Controllers
{
    public class CompaniesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
} 