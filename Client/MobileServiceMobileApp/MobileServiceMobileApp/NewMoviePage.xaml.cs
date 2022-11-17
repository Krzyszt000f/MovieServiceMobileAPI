using MovieService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace MobileServiceMobileApp {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewMoviePage : ContentPage {
        UserModel User;
        public NewMoviePage(UserModel user) {
            User = user;
            InitializeComponent();
        }

        void Cancel_Clicked(object sender, EventArgs e) {
            App.Current.MainPage = new AdminPage(User);
        }

        void Add_Clicked(object sender, EventArgs e) {
            MovieModel movieModel = new MovieModel();
            movieModel.title = TitleEntry.Text;
            movieModel.releaseDate = ReleaseDatePicker.Date.ToString("d.M.yyyy");
            movieModel.director = DirectorEntry.Text;
            movieModel.actors = ActorsEntry.Text;
            movieModel.description = DescriptionEditor.Text;

            if (movieModel.title.Equals("") || movieModel.director.Equals("") || movieModel.director.Equals("") || movieModel.actors.Equals("") || movieModel.description.Equals("")) {
                SetError("Some information are missing", true);
            } else {
                var result = PostMovie(movieModel);

                if (result.StatusCode == "201") {
                    SetError(result.Message, false);
                } else {
                    SetError(result.Message, true);
                }
            }
        }

        void SetError(string error, bool isError = true) {
            if (isError) ErrorLabel.TextColor = Color.Red;
            else ErrorLabel.TextColor = Color.Green;
            ErrorLabel.Text = error;
        }

        [HttpPost]
        public ResponseMessageStatus PostMovie(MovieModel movieModel) {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var uri = new Uri(Consts.URL + "/api/movies/edit");

                string token = User.accessToken;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var stringPayload = JsonConvert.SerializeObject(movieModel);
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                var response = client.PostAsync(uri, httpContent);
                var result = response.Result.Content.ReadAsStringAsync().Result;
                ResponseMessageStatus messageStatus = JsonConvert.DeserializeObject<ResponseMessageStatus>(result);
                return messageStatus;
            }
        }
    }
}