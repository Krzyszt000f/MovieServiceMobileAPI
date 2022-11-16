using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using MovieService.Models;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace MovieService.ViewControllers {
    public class AdminController : Controller {
        public IActionResult Index() {
            string cookie = Request.Cookies["refreshToken"];
            if (cookie == null) {
                ViewBag.ShowLoginRegister = true;
            } else {
                ViewBag.ShowLoginRegister = false;
            }
            List<MovieModel> movies = (List<MovieModel>)GetMovies().Result.Value;
            List<MovieModel> moviesDetails = new List<MovieModel>();
            foreach (var movie in movies) {
                moviesDetails.Add((MovieModel)GetMovies(movie.guid).Result.Value);
            }
            return View(moviesDetails);
        }

        public IActionResult Edit() {
            MovieModel movieModel = new MovieModel();
            movieModel.guid = Request.Query["guid"].ToString();
            movieModel.title = Request.Query["title"].ToString();
            movieModel.releaseDate = Request.Query["releaseDate"].ToString();
            movieModel.director = Request.Query["director"].ToString();
            movieModel.actors = Request.Query["actors"].ToString();
            movieModel.description = Request.Query["description"].ToString();

            if (ValidateDate(movieModel.releaseDate)) {
                var result = PutMovie(movieModel);
            }

            return Redirect(consts.URL + "/Home/Admin");
        }

        public IActionResult Delete() {
            MovieModel movieModel = new MovieModel();
            movieModel.guid = Request.Query["guid"].ToString();
            var result = DeleteMovie(movieModel);

            return Redirect(consts.URL + "/Home/Admin");
        }

        public IActionResult Add() {
            MovieModel movieModel = new MovieModel();
            movieModel.title = Request.Query["title"].ToString();
            movieModel.releaseDate = Request.Query["releaseDate"].ToString();
            movieModel.director = Request.Query["director"].ToString();
            movieModel.actors = Request.Query["actors"].ToString();
            movieModel.description = Request.Query["description"].ToString();

            if (ValidateDate(movieModel.releaseDate)) {
                var result = PostMovie(movieModel);
            }

            return Redirect(consts.URL + "/Home/Admin");
        }

        private bool ValidateDate(string date) {
            string[] formats = { "d.M.yyyy", "yyyy.M.d", "d-M-yyyy", "yyyy-M-d" };
            DateTime dateValue;

            if (DateTime.TryParseExact(date, formats, new CultureInfo("pl-PL"), DateTimeStyles.None, out dateValue))
                return true;
            else
                return false;
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

        [HttpPost]
        public ResponseMessageStatus PostMovie(MovieModel movieModel) {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var uri = new Uri(consts.URL + "/api/movies/edit");

                string token = Request.Cookies["accessToken"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var stringPayload = JsonConvert.SerializeObject(movieModel);
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                var response = client.PostAsync(uri, httpContent);
                var result = response.Result.Content.ReadAsStringAsync().Result;
                ResponseMessageStatus messageStatus = JsonConvert.DeserializeObject<ResponseMessageStatus>(result);
                return messageStatus;
            }
        }

        [HttpDelete]
        public ResponseMessageStatus DeleteMovie(MovieModel movieModel) {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var uri = new Uri(consts.URL + "/api/movies/edit");

                string token = Request.Cookies["accessToken"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var stringPayload = JsonConvert.SerializeObject(movieModel);
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage {
                    Method = HttpMethod.Delete,
                    RequestUri = uri,
                    Content = new StringContent(JsonConvert.SerializeObject(movieModel), Encoding.UTF8, "application/json")
                };

                var response = client.SendAsync(request);
                var result = response.Result.Content.ReadAsStringAsync().Result;
                ResponseMessageStatus messageStatus = JsonConvert.DeserializeObject<ResponseMessageStatus>(result);
                return messageStatus;
            }
        }

        [HttpPut]
        public ResponseMessageStatus PutMovie(MovieModel movieModel) {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var uri = new Uri(consts.URL + "/api/movies/edit");

                string token = Request.Cookies["accessToken"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var stringPayload = JsonConvert.SerializeObject(movieModel);
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                var response = client.PutAsync(uri, httpContent);
                var result = response.Result.Content.ReadAsStringAsync().Result;
                ResponseMessageStatus messageStatus = JsonConvert.DeserializeObject<ResponseMessageStatus>(result);
                return messageStatus;
            }
        }

    }
}
