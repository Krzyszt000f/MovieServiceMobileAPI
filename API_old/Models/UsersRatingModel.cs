using System.Text.Json.Serialization;

namespace MovieService.Models
{
    public class UsersRatingModel
    {
        [JsonIgnore]
        public string ratingGuid { get; set; }
        [JsonIgnore]
        public string userGuid { get; set; }
        [JsonIgnore]
        public string movieGuid { get; set; }
        public string rating { get; set; }
    }
}
