using MovieService.Models;

namespace MovieService
{
    public class RegisterDataTransferObject : UserModel
    {
        public string password { get; set; }
    }
}
