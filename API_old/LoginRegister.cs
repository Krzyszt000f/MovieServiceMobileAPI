using MovieService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Npgsql;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace MovieService
{
    public class LoginRegister : Controller
    {
        private readonly IConfiguration _configuration;

        public LoginRegister(IConfiguration config)
        {
            _configuration = config;
        }

        #region LoginRegister
        [Route("/api/login")]
        [HttpPost]
        public async Task<ActionResult<UserModel>> Login([FromBody] UserDataTransferObject request)
        {
            UserModel user = new UserModel();

            NpgsqlConnection conn = new NpgsqlConnection("User ID=postgres;Password=mysecretpassword;Host=localhost;Port=5432;Database=movieservice;");
            //NpgsqlConnection conn = new NpgsqlConnection("User ID=postgres;Password=123;Host=localhost;Port=5432;Database=moneyplusAlpha;");
            //NpgsqlConnection conn = new NpgsqlConnection("User ID=krzysztof_golusinski@moneyplus-server;Password=Am22Kg23;Host=moneyplus-server.postgres.database.azure.com;Port=5432;Database=moneyplus_db;");
            conn.Open();
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT * FROM users WHERE email='" + request.email + "'";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            if (reader.HasRows)
            {
                user.userGuid = reader["user_guid"].ToString();
                user.userName = reader["user_name"].ToString();
                user.userPassword = reader["password_hash"].ToString();
                user.email = reader["email"].ToString();
                reader.Close();
            }
            else
                return BadRequest(new ResponseMessageStatus { StatusCode = "400", Message = "Username or password is incorrect" });

            //bool passwordCorrect = VerifyPasswordHash(request.password, user.passwordHash, user.passwordSalt);
            if (user.email == request.email && user.userPassword == request.password)
            {
                Tokens token = new Tokens();
                token.accessToken = CreateToken(user);
                RefreshToken fulltoken = GenerateRefreshToken(user);
                token.refreshToken = fulltoken.Token;
                cmd.CommandText = "UPDATE users SET refresh_token = '" + fulltoken.Token + "', token_expires = '" + fulltoken.Expires.ToString("yyyy.MM.dd HH:mm:ss") + "' WHERE user_guid = '" + user.userGuid + "'"; //kurwa kto to projektował, żeby w dedykowanym środowisku silnik bazodanowy sobie sam parsował fulltoken.Expires, a tutaj trzeba to ręcznie robić ... postgres 11 < postgres 12
                cmd.ExecuteReader();
                return Ok(token);
            }
            else
                return BadRequest(new ResponseMessageStatus { StatusCode = "400", Message = "Username or password is incorrect" });
        }

        [Route("/api/register")]
        [HttpPost]
        public async Task<ActionResult<UserModel>> Register([FromBody] RegisterDataTransferObject request)
        {
            UserModel user = new UserModel();
            //CreatePasswordHash(request.password, out byte[] passwordHash, out byte[] passwordSalt);

            user.userName = request.userName;
            user.userPassword = request.password;
            user.email = request.email;

            NpgsqlConnection conn = new NpgsqlConnection("User ID=postgres;Password=mysecretpassword;Host=localhost;Port=5432;Database=movieservicemobile;");
            //NpgsqlConnection conn = new NpgsqlConnection("User ID=postgres;Password=123;Host=localhost;Port=5432;Database=moneyplusAlpha;");
            //NpgsqlConnection conn = new NpgsqlConnection("User ID=krzysztof_golusinski@moneyplus-server;Password=Am22Kg23;Host=moneyplus-server.postgres.database.azure.com;Port=5432;Database=moneyplus_db;");
            conn.Open();
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "INSERT INTO users(user_guid, user_name, user_password, email, user_role) VALUES ('" + Guid.NewGuid().ToString() + "', '" + user.userName + "', '" + user.userPassword + "', '" + user.email + "', 'Normal')";
            try
            {
                NpgsqlDataReader reader = cmd.ExecuteReader();
            }
            catch (NpgsqlException ex)
            {
                if (ex.SqlState.Equals("23505"))
                    return Conflict(new ResponseMessageStatus { StatusCode = "409", Message = "User with this email address already exists" });
            }
            return Created(string.Empty, new ResponseMessageStatus { StatusCode = "201", Message = "User created" });
        }
        #endregion

        #region PasswordHashes - XD not here
        //protected void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        //{
        //    using (var hmac = new HMACSHA512())
        //    {
        //        passwordSalt = hmac.Key;
        //        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        //    }
        //}

        //protected bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        //{
        //    using (var hmac = new HMACSHA512(passwordSalt))
        //    {
        //        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        //        return computedHash.SequenceEqual(passwordHash);
        //    }
        //}
        #endregion

        #region TokenOperations
        [Route("/api/refresh")]
        [HttpPost]
        public async Task<ActionResult<string>> RefreshToken([FromHeader] string refreshToken)
        {
            UserModel user = new UserModel();

            //NpgsqlConnection conn = new NpgsqlConnection("User ID=postgres;Password=123;Host=localhost;Port=5432;Database=moneyplusAlpha;");
            NpgsqlConnection conn = new NpgsqlConnection("User ID=krzysztof_golusinski@moneyplus-server;Password=Am22Kg23;Host=moneyplus-server.postgres.database.azure.com;Port=5432;Database=moneyplus_db;");
            conn.Open();
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select * from Users where refresh_token = '" + refreshToken + "'";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            reader.Read();

            if (reader.HasRows)
            {
                user.userGuid = reader["user_guid"].ToString();
                user.refreshToken = reader["refresh_token"].ToString();
                user.tokenExpires = Convert.ToDateTime(reader["token_expires"].ToString());
                reader.Close();
            }
            else
                return BadRequest(new ResponseMessageStatus { StatusCode = "400", Message = "Invalid refresh token or token expired" });

            Tokens tokens = new Tokens();
            if (!user.refreshToken.Equals(refreshToken) || user.tokenExpires < DateTime.Now)
            {
                return BadRequest(new ResponseMessageStatus { StatusCode = "400", Message = "Invalid refresh token or token expired" });
            }

            tokens.accessToken = CreateToken(user);
            RefreshToken fulltoken = GenerateRefreshToken(user);
            tokens.refreshToken = fulltoken.Token;
            cmd.CommandText = "UPDATE users SET refresh_token = '" + fulltoken.Token + "', token_expires = '" + fulltoken.Expires.ToString("yyyy.MM.dd HH:mm:ss") + "' WHERE user_guid = '" + user.userGuid + "'";
            cmd.ExecuteReader();

            return Ok(tokens);
        }

        private RefreshToken GenerateRefreshToken(UserModel user)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(2),
                Created = DateTime.Now
            };

            user.refreshToken = refreshToken.Token;
            user.tokenExpires = refreshToken.Expires;

            return refreshToken;
        }

        private string CreateToken(UserModel user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("user guid", user.userGuid),
                new Claim(ClaimTypes.Role, user.userRole)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
        #endregion
    }
}
