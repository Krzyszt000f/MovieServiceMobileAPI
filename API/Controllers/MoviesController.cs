using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieService.Models;
using Npgsql;
using System.Data;
using System.Security.Claims;
using System.Xml.Linq;

namespace MovieService.Controllers
{
    public class MoviesController : Controller
    {
        private readonly IConfiguration _configuration;

        public MoviesController(IConfiguration configuration) {
           _configuration = configuration;
        }

        [Route("/api/movies/{id?}")]
        [HttpGet]
        public List<MovieModel> GET(string? id)
        {
            //var id = User.FindFirstValue("user id");
            List<MovieModel> show_movies = new List<MovieModel>();
            NpgsqlConnection conn = new NpgsqlConnection(_configuration.GetSection("AppSettings:DbConnect").Value);
            NpgsqlParameter parametrized_id= new NpgsqlParameter("id", DbType.String);
            conn.Open();
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            if (id == null)
                cmd.CommandText = "select * from movies";
            else {
                cmd.CommandText = "select * from movies where movie_guid = @id";
                parametrized_id.Value = id;
                cmd.Parameters.Add(parametrized_id);
            }
            NpgsqlDataReader reader = cmd.ExecuteReader();
            if (id == null)
            {
                while(reader.Read())
                {
                    MovieModel movie = new MovieModel();
                    movie.guid = reader["movie_guid"].ToString();
                    movie.title = reader["title"].ToString();
                    movie.director = reader["director"].ToString();
                    movie.yearOfProduction = Convert.ToDateTime(reader["date_of_production"]).Year.ToString();
                    show_movies.Add(movie);
                }
            }
            else
            {
                reader.Read();
                MovieModel movie = new MovieModel();
                movie.guid = reader["movie_guid"].ToString();
                movie.title = reader["title"].ToString();
                movie.releaseDate = Convert.ToDateTime(reader["date_of_production"]).ToString("yyyy.MM.dd");
                movie.director = reader["director"].ToString();
                movie.actors = reader["actors"].ToString();
                movie.description = reader["description"].ToString();

                reader.Close();
                cmd.CommandText = "select users_comments.comment_guid, users.user_name, users_comments.movie_guid, users_comments.comment_content, users_comments.creation_date from users_comments inner join users on users_comments.user_guid = users.user_guid where users_comments.movie_guid=@id";
                reader = cmd.ExecuteReader();
                List<UsersCommentsModel> comments = new List<UsersCommentsModel>();
                while (reader.Read())
                {
                    if (reader["comment_content"] != null)
                    {
                        UsersCommentsModel comment = new UsersCommentsModel();
                        comment.commentGuid = reader["comment_guid"].ToString();
                        comment.userName = reader["user_name"].ToString();
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
        [HttpPost, HttpPut, HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MovieModel>> Edit([FromBody] MovieModel request)
        {
            //var id = User.FindFirstValue("user id");
            List<MovieModel> show_movies = new List<MovieModel>();
            NpgsqlConnection conn = new NpgsqlConnection(_configuration.GetSection("AppSettings:DbConnect").Value);
            NpgsqlParameter parametrized_id = new NpgsqlParameter("id", DbType.String);
            NpgsqlParameter parametrized_title = new NpgsqlParameter("title", DbType.String);
            NpgsqlParameter parametrized_director = new NpgsqlParameter("director", DbType.String);
            NpgsqlParameter parametrized_actors = new NpgsqlParameter("actors", DbType.String);
            NpgsqlParameter parametrized_description = new NpgsqlParameter("description", DbType.String);
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

                cmd.CommandText = "INSERT INTO movies (movie_guid, title, date_of_production, director, actors, description) VALUES ('" + Guid.NewGuid().ToString() + "', @title, '" + movie.releaseDate + "', @director, @actors, @description)";
                parametrized_title.Value = movie.title;
                cmd.Parameters.Add(parametrized_title);
                parametrized_director.Value = movie.director;
                cmd.Parameters.Add(parametrized_director);
                parametrized_actors.Value = movie.actors;
                cmd.Parameters.Add(parametrized_actors);
                parametrized_description.Value = movie.description;
                cmd.Parameters.Add(parametrized_description);
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

                cmd.CommandText = "DELETE FROM movies WHERE movie_guid = @id";
                parametrized_id.Value = movie.guid;
                cmd.Parameters.Add(parametrized_id);
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

                cmd.CommandText = "UPDATE movies SET title = @title, date_of_production = '" + movie.releaseDate + "', director = @director, actors = @actors, description = @description WHERE movie_guid = @id";
                parametrized_title.Value = movie.title;
                cmd.Parameters.Add(parametrized_title);
                parametrized_director.Value = movie.director;
                cmd.Parameters.Add(parametrized_director);
                parametrized_actors.Value = movie.actors;
                cmd.Parameters.Add(parametrized_actors);
                parametrized_description.Value = movie.description;
                cmd.Parameters.Add(parametrized_description);
                parametrized_id.Value = movie.guid;
                cmd.Parameters.Add(parametrized_id);
                NpgsqlDataReader reader = cmd.ExecuteReader();
                return Ok(new ResponseMessageStatus { StatusCode = "200", Message = "Movie updated" });
            }
            return View();
        }
    }
}
