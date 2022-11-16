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

namespace MobileServiceMobileApp {
    public partial class MoviePage : ContentPage {
        private UserModel user;
        public UserModel User {
            get {
                return user;
            }
            set {
                user = value;
                NewCommentStackPanel.IsVisible = user != null;
                NewCommentLabel.IsVisible = user == null;
                LoginButton.IsVisible = user == null;
                RegisterButton.IsVisible = user == null;
                LogoutButton.IsVisible = user != null;
                AdminButton.IsVisible = user != null && user.userRole.Equals("Admin");
            }
        }
        public MovieModel Movie;
        public ICollection<UsersCommentsModel> Comments;
        //public ICollection<UsersCommentsModel> Comments {
        //    get {
        //        return Movie.UsersComments;
        //    }
        //}

        public MoviePage(MovieModel movie, UserModel user) {
            InitializeComponent();

            User = user;
            Movie = movie;
            Comments = movie.UsersComments;
            MovieGrid.BindingContext = Movie;
            CommentsListView.ItemsSource = Comments;
            TitleLabel.BindingContext = Movie;


        }

        void NewComment_Clicked(object sender, EventArgs e) {
            string content = NewCommentContent.Text;
            string guid = Movie.guid;
            if (content != null && content != "") {
                UsersCommentsModel userComment = new UsersCommentsModel();

                userComment.commentContent = content;
                userComment.movieGuid = guid;

                var result = AddComment(userComment);

            }
            App.Current.MainPage = new MoviePage(Movie, User);
        }

        void Home_Clicked(object sender, EventArgs e) {
            App.Current.MainPage = new MainPage(user);
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

        void Admin_Clicked(object sender, EventArgs e) {
            App.Current.MainPage = new AdminPage();
        }

        [HttpPost]
        public ResponseMessageStatus AddComment(UsersCommentsModel userComment) {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            using (var client = new HttpClient(clientHandler)) {
                var uri = new Uri(Consts.URL + "/api/comment");

                string token = user.accessToken;
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