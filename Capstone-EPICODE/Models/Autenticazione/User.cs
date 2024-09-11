using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_EPICODE.Models
{
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string Name { get; set; }

        [Required, EmailAddress(ErrorMessage = "Tipo di email non valido")]
        public string Email { get; set; }

        [Required, Column(TypeName = "nvarchar(max)")]
        public string? Password { get; set; }

        public List<Role> Roles { get; set; } = new List<Role>();
    }
}
