
using System.ComponentModel.DataAnnotations;

namespace Capstone_EPICODE.Models
{
    public class UserViewModel
    {
        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [StringLength(30)]
        public required string Name { get; set; }

        [EmailAddress(ErrorMessage = "Tipo di indirizzo email non valida")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La password è obbligatoria")]
        [StringLength(30)]
        public required string Password { get; set; }


        public List<Role> Roles { get; set; } = new List<Role>();

      

    }
}
