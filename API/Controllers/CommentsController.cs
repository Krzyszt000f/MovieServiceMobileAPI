using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MovieService.Models;
using Npgsql;
using System.Data;
using System.Security.Claims;

namespace MovieService.Controllers
{
    public class CommentsController : Controller
    {
        private readonly IConfiguration _configuration;

        public CommentsController(IConfiguration configuration) {
            _configuration = configuration;
        }

        [Route("/api/comment")]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<UsersCommentsModel>> Comment([FromBody] UsersCommentsModel request)
        {
            var guid = User.FindFirstValue("user guid");
            List<MovieModel> show_movies = new List<MovieModel>();
            NpgsqlConnection conn = new NpgsqlConnection(_configuration.GetSection("AppSettings:DbConnect").Value);
            NpgsqlParameter parametrized_id = new NpgsqlParameter("id", DbType.String);
            NpgsqlParameter parametrized_movieId = new NpgsqlParameter("movieId", DbType.String);
            NpgsqlParameter parametrized_content = new NpgsqlParameter("content", DbType.String);
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

            cmd.CommandText = "INSERT INTO users_comments (comment_guid, user_guid, movie_guid, comment_content, creation_date) VALUES ('" + Guid.NewGuid().ToString() + "', @id, @movieId, @content, '" + comment.creationDate + "')";
            parametrized_id.Value = guid;
            cmd.Parameters.Add(parametrized_id);
            parametrized_movieId.Value = comment.movieGuid;
            cmd.Parameters.Add(parametrized_movieId);
            parametrized_content.Value = comment.commentContent;
            cmd.Parameters.Add(parametrized_content);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            return Ok(new ResponseMessageStatus { StatusCode = "201", Message = "Comment added" });
        }
    }
}
