using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO_EF.Data.Entities
{
    public class Contact
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }       
        public Guid ContactUserId { get; set; } 

        public string? Nickname { get; set; } 
        public DateTime AddedAt { get; set; }

  
        public User User { get; set; } = null!;
        public User ContactUser { get; set; } = null!;
    }
}
