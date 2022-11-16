using Microsoft.AspNetCore.Mvc;
using MovieService.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MovieService.ViewControllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) {
            _logger = logger;
        }

        public IActionResult Index() {
            SetNavPanel();
            var movies = GetMovies().Result.Value;
            return View(movies);
        }

        public IActionResult Privacy() {
            SetNavPanel();
            return View();
        }

        public IActionResult Login() {
            SetNavPanel();
            return View();
        }

        public IActionResult Register() {
            SetNavPanel();
            return View();
        }

        public IActionResult Logout() {
            Response.Cookies.Delete("refreshToken");
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("role");
            return Redirect(consts.URL);
        }

        public IActionResult Admin() {
            SetNavPanel();
            List<MovieModel> movies = (List<MovieModel>)GetMovies().Result.Value;
            List<MovieModel> moviesDetails = new List<MovieModel>();
            foreach (var movie in movies) {
                moviesDetails.Add((MovieModel)GetMovies(movie.guid).Result.Value);
            }
            return View(moviesDetails);
        }

        public IActionResult Movie(string guid) {
            SetNavPanel();

            if (guid != null && guid != "") {
                var movie = GetMovies(guid).Result.Value;
                return View(movie);
            } else {
                return Redirect(consts.URL);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void SetNavPanel() {
            string accessToken = Request.Cookies["accessToken"];
            string role = Request.Cookies["role"];
            if (accessToken == null) {
                ViewBag.ShowLoginRegister = true;
            } else {
                ViewBag.ShowLoginRegister = false;
            }
            if (role != null && role.Equals("Admin")) {
                ViewBag.ShowAdmin = true;
            } else {
                ViewBag.ShowAdmin = false;
            }
        }

        [HttpGet]
        public async Task<OkObjectResult> GetMovies(string guid = "") {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var baseUri = $"{Request.Scheme}://{Request.Host}";
                var uri = new Uri(baseUri + "/api/movies/" + guid);
                var response = await client.GetAsync(uri);
                string textResult = await response.Content.ReadAsStringAsync();

                List<MovieModel> movies = JsonConvert.DeserializeObject<List<MovieModel>>(textResult);

                if (!guid.Equals("") && movies != null && movies.Count > 0) {
                    return Ok(movies[0]);
                } else {
                    return Ok(movies);
                }
            }
        }
    }
}