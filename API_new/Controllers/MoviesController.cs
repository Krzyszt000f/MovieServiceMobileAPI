using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieService.Models;
using Npgsql;
using System.Data;
using System.Security.Claims;

namespace MovieService.Controllers
{
    public class MoviesController : Controller
    {

        [Route("/api/movies/{id?}")]
        [HttpGet]
        public List<MovieModel> GET(string? id)
        {
            //var id = User.FindFirstValue("user id");
            List<MovieModel> show_movies = new List<MovieModel>();
            NpgsqlConnection conn = new NpgsqlConnection("User ID=postgres;Password=mysecretpassword;Host=localhost;Port=5432;Database=movieservice;");
            //NpgsqlConnection conn = new NpgsqlConnection("User ID=krzysztof_golusinski@moneyplus-server;Password=Am22Kg23;Host=moneyplus-server.postgres.database.azure.com;Port=5432;Database=moneyplus_db;");
            conn.Open();
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            if (id == null)
                cmd.CommandText = "select * from movies";
            else
                cmd.CommandText = "select * from movies where movie_guid = '" + id + "'";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            if (id == null)
            {
                while(reader.Read())
                {
                    MovieModel movie = new MovieModel();
                    movie.guid = reader["movie_guid"].ToString();
                    movie.title = reader["title"].ToString();
                    movie.director = reader["director"].ToString();
                    movie.rating = reader["rating"].ToString();
                    movie.yearOfProduction = Convert.ToDateTime(reader["date_of_production"]).Year.ToString();
                    show_movies.Add(movie);
                }
            }
            else
            {
                reader.Read();
                MovieModel movie = new MovieModel();
                movie.title = reader["title"].ToString();
                movie.releaseDate = Convert.ToDateTime(reader["date_of_production"]).ToString("yyyy.MM.dd");
                movie.director = reader["director"].ToString();
                movie.actors = reader["actors"].ToString();
                movie.rating = reader["rating"].ToString();
                movie.description = reader["description"].ToString();

                reader.Close();
                cmd.CommandText = "select * from users_comments where movie_guid = '" + id + "'";
                reader = cmd.ExecuteReader();
                List<UsersCommentsModel> comments = new List<UsersCommentsModel>();
                while (reader.Read())
                {
                    if (reader["comment_content"] != null)
                    {
                        UsersCommentsModel comment = new UsersCommentsModel();
                        comment.commentGuid = reader["comment_guid"].ToString();
                        comment.userGuid = reader["user_guid"].ToString();
                        comment.commentContent = reader["comment_content"].ToString();
                        comment.creationDate = Convert.ToDateTime(reader["creation_date"].ToString());

                        comments.Add(comment);
                    }
                }
                movie.UsersComments = comments;

                show_movies.Add(movie);
            }

            return show_movies;
        }

        [Route("/api/movies/edit")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MovieModel>> Edit([FromBody] MovieModel request)
        {
            //var id = User.FindFirstValue("user id");
            List<MovieModel> show_movies = new List<MovieModel>();
            NpgsqlConnection conn = new NpgsqlConnection("User ID=postgres;Password=123;Host=localhost;Port=5432;Database=moneyplusAlpha;");
            //NpgsqlConnection conn = new NpgsqlConnection("User ID=krzysztof_golusinski@moneyplus-server;Password=Am22Kg23;Host=moneyplus-server.postgres.database.azure.com;Port=5432;Database=moneyplus_db;");
            conn.Open();
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;

            if (Request.Method == "POST")
            {
                MovieModel movie = new MovieModel();

                try
                {
                    movie.title = request.title;
                    movie.releaseDate = request.releaseDate;
                    movie.director = request.director;
                    movie.actors = request.actors;
                    movie.description = request.description;
                }
                catch (Exception ex)
                {
                    return BadRequest(new ResponseMessageStatus { StatusCode = "400", Message = "Invalid data type" });
                }

                cmd.CommandText = "INSERT INTO movies (movie_guid, title, date_of_production, actors, description) VALUES ('" + Guid.NewGuid().ToString() + "', '" + movie.title + "', '" + movie.releaseDate + "', '" + movie.actors + "', '" + movie.description + "')";
                NpgsqlDataReader reader = cmd.ExecuteReader();
                return Ok(new ResponseMessageStatus { StatusCode = "201", Message = "Movie added" });
            }
            else if (Request.Method == "DELETE")
            {
                MovieModel movie = new MovieModel();

                try
                {
                    movie.guid = request.guid;
                }
                catch (Exception ex)
                {
                    return BadRequest(new ResponseMessageStatus { StatusCode = "400", Message = "Invalid data type" });
                }

                cmd.CommandText = "DELETE FROM movies WHERE movie_guid = '" + movie.guid + "'";
                NpgsqlDataReader reader = cmd.ExecuteReader();
                return Ok(new ResponseMessageStatus { StatusCode = "200", Message = "Movie deleted" });
            }
            else if (Request.Method == "PUT")
            {
                MovieModel movie = new MovieModel();

                try
                {
                    movie.guid = request.guid;
                    movie.title = request.title;
                    movie.releaseDate = request.releaseDate;
                    movie.director = request.director;
                    movie.actors = request.actors;
                    movie.description = request.description;
                }
                catch (Exception ex)
                {
                    return BadRequest(new ResponseMessageStatus { StatusCode = "400", Message = "Invalid data type" });
                }

                cmd.CommandText = "UPDATE movies SET title = '" + movie.title + "', date_of_production = '" + movie.releaseDate + "', director = '" + movie.director + "', actors = '" + movie.actors + "', description = '" + movie.description + " WHERE movie_guid = '" + movie.guid + "'";
                NpgsqlDataReader reader = cmd.ExecuteReader();
                return Ok(new ResponseMessageStatus { StatusCode = "200", Message = "Movie updated" });
            }
            return View();
        }
    }
}
