using Microsoft.AspNetCore.Mvc;
using MovieService.Models;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Text;

namespace MovieService.ViewControllers
{
    public class RegisterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Submit() {
            var username = Request.Query["username"].ToString();
            var email = Request.Query["email"].ToString();
            var password = Request.Query["password"].ToString();
            var repeatPassword = Request.Query["repeatpassword"].ToString();

            if (ValidateEmail(email) && !string.Equals(password, repeatPassword) && ValidatePassword(password)) {            
                UserDataTransferObject userDataTransfer = new UserDataTransferObject();

                userDataTransfer.username = username;
                userDataTransfer.email = email;
                userDataTransfer.password = password;

                var result = Register(userDataTransfer);

                return Redirect(consts.URL);
            } else {
                return Redirect(consts.URL+"/Home/Register");
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

        bool ValidatePassword(string password) {
            if (password.Length > 7 && password.Any(char.IsDigit) && password.Any(char.IsLetter)) {
                return true;
            } else {
                return false;
            }
        }

        [HttpPost]
        public ResponseMessageStatus Register(UserDataTransferObject userDataTransfer) {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var uri = new Uri(consts.URL + "/api/register/");

                var stringPayload = JsonConvert.SerializeObject(userDataTransfer);
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                var response = client.PostAsync(uri, httpContent);
                var result = response.Result.Content.ReadAsStringAsync().Result;
                ResponseMessageStatus messageStatus = JsonConvert.DeserializeObject<ResponseMessageStatus>(result);
                return messageStatus;
            }
        }
    }
}
