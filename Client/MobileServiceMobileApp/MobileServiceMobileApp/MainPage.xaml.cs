using Microsoft.AspNetCore.Mvc;
using MovieService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Globalization;
using Xamarin.CommunityToolkit;

namespace MobileServiceMobileApp {
    public partial class MainPage : ContentPage {
        string url = "https://192.168.1.13:7277";
        public List<MovieModel> Movies;
        private UserModel user;
        public UserModel User {
            get {
                return user;
            }
            set {
                user = value;
                LoginButton.IsVisible = user == null;
                RegisterButton.IsVisible = user == null;
                LogoutButton.IsVisible = user != null;
            }
        }

        public MainPage(UserModel _user = null) {          
            InitializeComponent();
            User = _user;

            Movies = GetMovies();
            MoviesListView.ItemsSource = Movies;
        }

        void Home_Clicked(object sender, System.EventArgs e) {
            App.Current.MainPage = new MainPage(user);
        }

        void Login_Clicked(object sender, System.EventArgs e) {
            App.Current.MainPage = new LoginPage();
        }

        void Register_Clicked(object sender, System.EventArgs e) {
            App.Current.MainPage = new RegisterPage();
        }

        void Logout_Clicked(object sender, System.EventArgs e) {
            App.Current.MainPage = new MainPage();
        }

        void OnTapMovie(object sender, System.EventArgs e) {
            Label senderLabel = (Label)sender;
            string title = senderLabel.Text;
            string guid = Movies.First(o => o.title == title).guid;
            var movie = GetMovies(guid);
            App.Current.MainPage = new MoviePage(movie[0]);
        }

        [HttpGet]
        public List<MovieModel> GetMovies(string guid = "") {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var baseUri = url;
                var uri = new Uri(baseUri + "/api/movies/" + guid);
                var response = client.GetAsync(uri);


                var result = response.Result.Content.ReadAsStringAsync().Result;

                List<MovieModel> movies = JsonConvert.DeserializeObject<List<MovieModel>>(result);


                // string textResult = response.Content.ReadAsStringAsync();
                /*
                List<MovieModel> movies = JsonConvert.DeserializeObject<List<MovieModel>>(textResult);

                if (!guid.Equals("") && movies != null && movies.Count > 0) {
                    return new OkObjectResult(movies[0]);
                } else {
                    return new OkObjectResult(movies);
                }
                */

                // List<MovieModel> movies = new List<MovieModel>();
                return movies;
            }
        }

    }

}
