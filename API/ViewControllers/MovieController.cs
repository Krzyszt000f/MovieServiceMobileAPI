using Microsoft.AspNetCore.Mvc;
using MovieService.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MovieService.ViewControllers {
    public class MovieController : Controller {
        public MovieModel movieModel;

        public IActionResult Index() {
            string cookie = Request.Cookies["accessToken"];
            if (cookie == null) {
                ViewBag.ShowLoginRegister = true;
            } else {
                ViewBag.ShowLoginRegister = false;
            }
            return View();
        }

        public IActionResult Submit() {
            string content = Request.Query["comment"].ToString();
            string guid = Request.Query["guid"].ToString();
            if (content != null && content != "") {
                UsersCommentsModel userComment = new UsersCommentsModel();

                userComment.commentContent = content;
                userComment.movieGuid = guid;

                var result = AddComment(userComment);

            }
            return Redirect(consts.URL+"/Home/Movie?guid="+guid);
        }

        [HttpPost]
        public ResponseMessageStatus AddComment(UsersCommentsModel userComment) {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var uri = new Uri(consts.URL + "/api/comment");

                string token = Request.Cookies["accessToken"];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var stringPayload = JsonConvert.SerializeObject(userComment);
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                var response = client.PostAsync(uri, httpContent);
                var result = response.Result.Content.ReadAsStringAsync().Result;
                ResponseMessageStatus messageStatus = JsonConvert.DeserializeObject<ResponseMessageStatus>(result);
                return messageStatus;
            }
        }
    }
}
