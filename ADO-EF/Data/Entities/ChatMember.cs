using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO_EF.Data.Entities
{
    public class ChatMember
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid UserId { get; set; }

        public DateTime JoinedAt { get; set; }
        public bool IsAdmin { get; set; } = false;

        
        public Chat Chat { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
