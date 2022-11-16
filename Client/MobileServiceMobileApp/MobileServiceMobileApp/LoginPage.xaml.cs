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
using System.Security.Cryptography;

namespace MobileServiceMobileApp {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage {
        UserModel user;

        public LoginPage() {
            InitializeComponent();
        }

        void Login_Clicked(object sender, EventArgs e) {
            if (Email.Text.Equals("") || Password.Text.Equals("")) {
                SetError("Email or password missing");
            } else if (!ValidateEmail(Email.Text)) {
                SetError("Inproper email address format");
            } else {
                UserDataTransferObject userDataTransfer = new UserDataTransferObject();
                //CreatePasswordHash(Password.Text, out byte[] passwordHash, out byte[] passwordSalt);

                userDataTransfer.email = Email.Text;
                userDataTransfer.password = Password.Text;
                //user.passwordHash = passwordHash;
                //user.passwordSalt = passwordSalt;

                var result = Login(userDataTransfer);

                if (result.accessToken != null && result.refreshToken != null) {
                    SetError("Logged in successfully", false);
                    user = new UserModel();
                    user.refreshToken = result.refreshToken;
                    user.accessToken = result.accessToken;
                    user.userRole = result.role;
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
        public ResponseMessageStatus Login(UserDataTransferObject userDataTransfer) {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var baseUri = Consts.URL;
                var uri = new Uri(baseUri + "/api/login/");

                var stringPayload = JsonConvert.SerializeObject(userDataTransfer);
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                var response = client.PostAsync(uri, httpContent);
                var result = response.Result.Content.ReadAsStringAsync().Result;
                ResponseMessageStatus messageStatus = JsonConvert.DeserializeObject<ResponseMessageStatus>(result);
                return messageStatus;
            }
        }

        void Home_Clicked(object sender, EventArgs e) {
            App.Current.MainPage = new MainPage(user);
        }

        void Register_Clicked(object sender, EventArgs e) {
            App.Current.MainPage = new RegisterPage();
        }
    }
}