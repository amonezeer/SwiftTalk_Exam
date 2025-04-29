using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO_EF.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phonenumber { get; set; }
        public DateTime? Birthdate { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public List<UserAccess> UserAccesses { get; set; } = [];
        public List<UserImage> UserImages { get; set; } = [];
        public List<ChatMember> ChatMembers { get; set; } = [];
        public List<Message> SentMessages { get; set; } = [];
        public List<Contact> Contacts { get; set; } = [];
        public List<Contact> ContactedBy { get; set; } = [];
        public List<Notification> Notifications { get; set; } = [];
    }
}
