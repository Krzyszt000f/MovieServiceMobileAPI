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
    public partial class EditMoviePanel : ContentPage {
        UserModel User;
        MovieModel Movie;
        public EditMoviePanel(UserModel user, MovieModel movie) {
            User = user;
            
            InitializeComponent();
            Movie = movie;
            MovieStackLayout.BindingContext = Movie;
        }

        void Cancel_Clicked(object sender, EventArgs e) {
            App.Current.MainPage = new AdminPage(User);
        }

        void Edit_Clicked(object sender, EventArgs e) {
            Movie.title = TitleEntry.Text;
            Movie.releaseDate = ReleaseDatePicker.Date.ToString("d.M.yyyy");
            Movie.director = DirectorEntry.Text;
            Movie.actors = ActorsEntry.Text;
            Movie.description = DescriptionEditor.Text;

            if (Movie.title.Equals("") || Movie.director.Equals("") || Movie.director.Equals("") || Movie.actors.Equals("") || Movie.description.Equals("")) {
                SetError("Some information are missing", true);
            } else {
                var result = PutMovie(Movie);

                if (result.StatusCode.Equals("200")) {
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

        [HttpPut]
        public ResponseMessageStatus PutMovie(MovieModel movieModel) {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var uri = new Uri(Consts.URL + "/api/movies/edit");

                string token = User.accessToken;
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