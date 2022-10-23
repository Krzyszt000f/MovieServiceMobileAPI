using MovieService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileServiceMobileApp {
    public partial class MoviePage : ContentPage {
        public MovieModel Movie;

        public MoviePage(MovieModel movie) {
            InitializeComponent();

            Movie = movie;
            MovieGrid.BindingContext = Movie;
            TitleLabel.BindingContext = Movie;
        }

        void Home_Clicked(object sender, System.EventArgs e) {
            App.Current.MainPage = new MainPage();
        }

        void Login_Clicked(object sender, System.EventArgs e) {
            App.Current.MainPage = new LoginPage();
        }

        void Register_Clicked(object sender, System.EventArgs e) {
            App.Current.MainPage = new RegisterPage();
        }
    }
}