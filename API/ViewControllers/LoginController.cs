using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using MovieService.Models;
using Newtonsoft.Json;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace MovieService.ViewControllers {
    public class LoginController : Controller {
        UserModel user;

        public IActionResult Index() {
            return View();
        }

        public IActionResult Submit() {
            var email = Request.Query["email"].ToString();
            var password = Request.Query["password"].ToString();

            if (!email.Equals("") && !password.Equals("") && ValidateEmail(email)) {
                UserDataTransferObject userDataTransfer = new UserDataTransferObject();

                userDataTransfer.email = email;
                userDataTransfer.password = password;

                var result = Login(userDataTransfer);

                if (result.accessToken != null && result.refreshToken != null && result.role != null) {
                    user = new UserModel();
                    user.refreshToken = result.refreshToken;
                    user.accessToken = result.accessToken;

                    CookieOptions option = new CookieOptions();
                    option.Expires = DateTime.Now.AddMilliseconds(100000);
                    Response.Cookies.Append("refreshToken", user.refreshToken, option);
                    Response.Cookies.Append("accessToken", user.accessToken, option);
                    Response.Cookies.Append("role", result.role, option);
                }
            }
            return Redirect(consts.URL);
        }

        bool ValidateEmail(string email) {
            try {
                MailAddress m = new MailAddress(email);
                return true;
            } catch (FormatException) {
                return false;
            }
        }

        [HttpPost]
        public Tokens Login(UserDataTransferObject userDataTransfer) {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var uri = new Uri(consts.URL + "/api/login/");

                var stringPayload = JsonConvert.SerializeObject(userDataTransfer);
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                var response = client.PostAsync(uri, httpContent);
                var result = response.Result.Content.ReadAsStringAsync().Result;
                Tokens messageStatus = JsonConvert.DeserializeObject<Tokens>(result);
                return messageStatus;
            }
        }
    }
}
