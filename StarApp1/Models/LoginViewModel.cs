using System.ComponentModel.DataAnnotations;

namespace StarApp1.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Key]
       
        public int UserId   { get; set; }
        [Required]
        
        
        public string UserName { get; set; }
       
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
        public int RoleId { get; set; }
    }
}
