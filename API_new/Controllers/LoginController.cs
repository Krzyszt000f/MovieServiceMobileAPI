using Microsoft.AspNetCore.Mvc;

namespace MovieService.Controllers {
    public class LoginController : Controller {
        public IActionResult Index() {
            return View();
        }
    }
}
