using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO_EF.Data.Entities
{
    public class Chat
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsGroup { get; set; }  

        public DateTime CreatedAt { get; set; }

       
        public List<ChatMember> Members { get; set; } = [];
        public List<Message> Messages { get; set; } = [];
    }
}
