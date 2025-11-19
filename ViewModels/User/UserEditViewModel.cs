using System.ComponentModel.DataAnnotations;

namespace QuizWeb_TrioForce.ViewModels.User
{
    public class UserEditViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = null!;

        [Required]
        [Display(Name = "Full Name")]
        public string Fullname { get; set; } = null!;

        [Display(Name = "Birthday")]
        public string Birthday { get; set; } = null!;

        [Display(Name = "Sex")]
        public bool Sex { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; } = null!;
    }
}
