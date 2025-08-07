using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheVault.Entities
{
    public class User
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
