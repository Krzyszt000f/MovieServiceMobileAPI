using MovieService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace MobileServiceMobileApp {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdminPage : ContentPage {
        public UserModel User;
        public List<MovieModel> Movies;
        public AdminPage(UserModel user) {
            User = user;
            InitializeComponent();
            Movies = GetMovies();
            MoviesListView.ItemsSource = Movies;
        }

        private void NewMovieButton_Clicked(object sender, EventArgs e) {
            App.Current.MainPage = new NewMoviePage(User);
        }

        private void EditMovieButton_Clicked(object sender, EventArgs e) {
            MovieModel model = (MovieModel)((Button)sender).BindingContext;
            App.Current.MainPage = new EditMoviePanel(User, GetMovies(model.guid)[0]);
        }

        async private void DeleteMovieButton_Clicked(object sender, EventArgs e) {
            MovieModel model = (MovieModel)((Button)sender).BindingContext;
            bool answer = await DisplayAlert("Confirm", "Do you want to delete movie \"" + model.title + "\"?", "Yes", "No");
            if (answer) {
                var result = DeleteMovie(model);
                if (result.StatusCode.Equals("200")) {
                    await DisplayAlert("Success", "Movie deleted", "OK");
                } else {
                    await DisplayAlert("Error", "Couldn't delete movie", "OK");
                }
                App.Current.MainPage = new AdminPage(User);
            }
        }

        [HttpGet]
        public List<MovieModel> GetMovies(string guid = "") {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var baseUri = Consts.URL;
                var uri = new Uri(baseUri + "/api/movies/" + guid);
                var response = client.GetAsync(uri);

                var result = response.Result.Content.ReadAsStringAsync().Result;

                List<MovieModel> movies = JsonConvert.DeserializeObject<List<MovieModel>>(result);
                return movies;
            }
        }

        [HttpDelete]
        public ResponseMessageStatus DeleteMovie(MovieModel movieModel) {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var uri = new Uri(Consts.URL + "/api/movies/edit");

                string token = User.accessToken;
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

        void Home_Clicked(object sender, EventArgs e) {
            App.Current.MainPage = new MainPage(User);
        }

        void Login_Clicked(object sender, EventArgs e) {
            App.Current.MainPage = new LoginPage();
        }

        void Register_Clicked(object sender, EventArgs e) {
            App.Current.MainPage = new RegisterPage();
        }

        void Logout_Clicked(object sender, EventArgs e) {
            App.Current.MainPage = new MainPage();
        }
    }
}