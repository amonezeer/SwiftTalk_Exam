using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO_EF.Data.Entities
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid SenderId { get; set; }

        public string Text { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

      
        public Chat Chat { get; set; } = null!;
        public User Sender { get; set; } = null!;
    }
}
