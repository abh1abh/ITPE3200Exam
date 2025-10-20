using Microsoft.AspNetCore.Mvc;

namespace HomecareAppointmentmanagement.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index() // Home page
        {
            return View();
        }
    }
}