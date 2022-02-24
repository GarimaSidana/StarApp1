using System.ComponentModel.DataAnnotations;

namespace StarApp1.Models
{
    public class UserAdmin
    {
        
        [Key]
        public string UserName { get; set; }
        public string Name { get; set; }
        public string ActiveFrom { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string Confirmation { get; set; }


    }
}
