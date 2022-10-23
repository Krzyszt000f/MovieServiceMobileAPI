using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MovieService.Models
{
    public class UserModel
    {
        [Key]
        public string userGuid { get; set; }
        public string userName { get; set; }
        [JsonIgnore]
        public string password { get; set; }
        public string email { get; set; }
        [JsonIgnore]
        public string userRole { get; set; }
        [JsonIgnore]
        public string refreshToken { get; set; }
        [JsonIgnore]
        public DateTime tokenExpires { get; set; }
    }
}
