using Microsoft.AspNetCore.Mvc;
using MovieService.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MovieService.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) {
            _logger = logger;
        }

        public IActionResult Index() {
            var movies = GetMovies().Result.Value;
            return View(movies);
            // List<MovieModel> movies = new List<MovieModel>();
            // return View(movies);
        }

        public IActionResult Privacy() {
            return View();
        }

        public IActionResult Login() {
            return View();
        }

        public IActionResult Register() {
            return View();
        }

        public IActionResult Movie(string guid) {
            var movie = GetMovies(guid).Result.Value;
            return View(movie);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<OkObjectResult> GetMovies(string guid = "") {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var baseUri = $"{Request.Scheme}://{Request.Host}";
                var uri = new Uri(baseUri+"/api/movies/" + guid);
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