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
    public partial class RegisterPage : ContentPage {

        public RegisterPage() {
            InitializeComponent();
        }

        void Register_Clicked(object sender, EventArgs e) {
            if (Username.Text.Equals("") || Email.Text.Equals("") || Password.Text.Equals("") || RepeatPassword.Text.Equals("")) {
                SetError("Information missing");
            } else if (!ValidateEmail(Email.Text)) {
                SetError("Inproper email address format");
            } else if (!string.Equals(Password.Text, RepeatPassword.Text)) {
                SetError("Passwords are different");
            } else if (!ValidatePassword(Password.Text)) {
                SetError("Password must contain at least 8 characters, one digit and one letter");
            } else {
                RegisterDataTransferObject userDataTransfer = new RegisterDataTransferObject();
                userDataTransfer.userName = Username.Text;
                userDataTransfer.email = Email.Text;
                userDataTransfer.userPassword = Password.Text;

                var result = Register(userDataTransfer);

                if (result.StatusCode == "201") {
                    SetError("User created - you can log in now", false);
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

        bool ValidateEmail (string email) {
            try {
                MailAddress m = new MailAddress(email);
                return true;
            } catch (FormatException) {
                return false;
            }
        }

        bool ValidatePassword (string password) {
            if (password.Length > 7 && password.Any(char.IsDigit) && password.Any(char.IsLetter)) {
                return true;
            } else {
                return false;
            }
        }

        [HttpPost]
        public ResponseMessageStatus Register(RegisterDataTransferObject user) {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var baseUri = Consts.URL;
                var uri = new Uri(baseUri + "/api/register/");

                var stringPayload = JsonConvert.SerializeObject(user);
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                var response = client.PostAsync(uri, httpContent);
                var result = response.Result.Content.ReadAsStringAsync().Result;
                ResponseMessageStatus messageStatus = JsonConvert.DeserializeObject<ResponseMessageStatus>(result);
                return messageStatus;
            }
        }

        void Home_Clicked(object sender, EventArgs e) {
            App.Current.MainPage = new MainPage();
        }

        void Login_Clicked(object sender, EventArgs e) {
            App.Current.MainPage = new LoginPage();
        }
    }
}