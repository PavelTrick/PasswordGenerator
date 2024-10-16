using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PasswordGenerator.Server.DAL.Models
{
    public class Password
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CodeHashCounter { get; set; }

        public ICollection<UserPassword> UserPasswords { get; set; } = new List<UserPassword>();

    }
}
