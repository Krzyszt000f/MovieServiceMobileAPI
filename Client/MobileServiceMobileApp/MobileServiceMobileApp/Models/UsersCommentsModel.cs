using System;
using System.Text.Json.Serialization;

namespace MovieService.Models
{
    public class UsersCommentsModel
    {
        [JsonIgnore]
        public string commentGuid { get; set; }
        public string userGuid { get; set; }
        [JsonIgnore]
        public string movieGuid { get; set; }
        public string commentContent { get; set; }
        public DateTime creationDate { get; set; }
    }
}
