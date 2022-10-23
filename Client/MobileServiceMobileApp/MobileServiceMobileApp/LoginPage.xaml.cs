using MovieService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace MobileServiceMobileApp {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage {
        readonly string Url = "https://192.168.1.13:7277";

        public LoginPage() {
            InitializeComponent();
        }

        void Login_Clicked(object sender, System.EventArgs e) {
            if (Email.Text.Equals("") || Password.Text.Equals("")) {
                SetError("Information missing");
            } else if (!ValidateEmail(Email.Text)) {
                SetError("Inproper email address format");
            } else {
                UserModel user = new UserModel();
                user.email = Email.Text;
                user.password = Password.Text;

                var result = Login(user);

                if (result.accessToken != null && result.refreshToken != null) {
                    SetError("Logged in successfully", false);
                    user.refreshToken = result.refreshToken;
                } else {
                    SetError(result.Message);
                }
            }
        }

        void SetError(string error, bool isError = true) {
            if (isError) ErrorLabel.TextColor = Color.Red;
            else ErrorLabel.TextColor = Color.Green;
            ErrorLabel.Text = error;
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
        public ResponseMessageStatus Login(UserModel user) {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var baseUri = Url;
                var uri = new Uri(baseUri + "/api/login/");

                var stringPayload = JsonConvert.SerializeObject(user);
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                var response = client.PostAsync(uri, httpContent);
                var result = response.Result.Content.ReadAsStringAsync().Result;
                ResponseMessageStatus messageStatus = JsonConvert.DeserializeObject<ResponseMessageStatus>(result);
                return messageStatus;
            }
        }

        void Home_Clicked(object sender, System.EventArgs e) {
            App.Current.MainPage = new MainPage();
        }

        void Register_Clicked(object sender, System.EventArgs e) {
            App.Current.MainPage = new RegisterPage();
        }
    }
}