using Microsoft.AspNetCore.Mvc;

namespace MovieService.Controllers {
    public class RegisterController : Controller {
        public IActionResult Index() {
            return View();
        }
    }
}
