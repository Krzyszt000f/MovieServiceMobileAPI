using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieService.Models;
using Npgsql;
using System.Data;
using System.Security.Claims;

namespace MovieService.Controllers
{
    public class CommentsRatingController : Controller
    {
        [Authorize("/api/comment")]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<UsersCommentsModel>> Comment([FromBody] UsersCommentsModel request)
        {
            var guid = User.FindFirstValue("user guid");
            List<MovieModel> show_movies = new List<MovieModel>();
            NpgsqlConnection conn = new NpgsqlConnection("User ID=postgres;Password=123;Host=localhost;Port=5432;Database=movieservice;");
            //NpgsqlConnection conn = new NpgsqlConnection("User ID=krzysztof_golusinski@moneyplus-server;Password=Am22Kg23;Host=moneyplus-server.postgres.database.azure.com;Port=5432;Database=moneyplus_db;");
            conn.Open();
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;

            UsersCommentsModel comment = new UsersCommentsModel();

            try
            {
                comment.movieGuid = request.movieGuid;
                comment.commentContent = request.commentContent;
                comment.creationDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessageStatus { StatusCode = "400", Message = "Invalid data type" });
            }

            cmd.CommandText = "INSERT INTO users_comments (comment_guid, user_guid, movie_guid, comment_content, creation_date) VALUES ('" + Guid.NewGuid().ToString() + "', '" + guid + "', '" + comment.movieGuid + "', '" + comment.commentContent + "', '" + comment.creationDate + "')";//tu to trzeba potem na pg11 przystosowac
            NpgsqlDataReader reader = cmd.ExecuteReader();
            return Ok(new ResponseMessageStatus { StatusCode = "201", Message = "Comment added" });
        }

        [Route("/api/rate")]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<UsersRatingModel>> Rate([FromBody] UsersRatingModel request)
        {
            var guid = User.FindFirstValue("user guid");
            List<MovieModel> show_movies = new List<MovieModel>();
            NpgsqlConnection conn = new NpgsqlConnection("User ID=postgres;Password=123;Host=localhost;Port=5432;Database=movieservice;");
            //NpgsqlConnection conn = new NpgsqlConnection("User ID=krzysztof_golusinski@moneyplus-server;Password=Am22Kg23;Host=moneyplus-server.postgres.database.azure.com;Port=5432;Database=moneyplus_db;");
            conn.Open();
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;

            UsersRatingModel rating = new UsersRatingModel();

            try
            {
                rating.movieGuid = request.movieGuid;
                rating.rating = request.rating;
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessageStatus { StatusCode = "400", Message = "Invalid data type" });
            }

            cmd.CommandText = "SELECT * FROM users_ratings WHERE user_guid = '" + guid + "' AND movie_guid = '" + rating.movieGuid + "'";
            NpgsqlDataReader reader = cmd.ExecuteReader();

            if(reader.HasRows)
            {
                cmd.CommandText = "UPDATE users_ratings SET rating = '" + rating.rating + "' WHERE comment_guid = '" + reader["comment_guid"].ToString() + "'";
                reader.Close();
                reader = cmd.ExecuteReader();
            }
            else
            {
                reader.Close();
                cmd.CommandText = "INSERT INTO users_ratings (rating_guid, user_guid, movie_guid, rating) VALUES ('" + Guid.NewGuid().ToString() + "', '" + guid + "', '" + rating.movieGuid + "', '" + rating.rating + "')";
                reader = cmd.ExecuteReader();
            }
            return Ok(new ResponseMessageStatus { StatusCode = "201", Message = "Rating added" });
        }
    }
}
