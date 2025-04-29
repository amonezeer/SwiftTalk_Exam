using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADO_EF.Data.Entities
{
    public class UserAccess
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string RoleId { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Salt { get; set; } = null!;
        public string Dk { get; set; } = null!;

        public User User { get; set; } = null!;
        public UserRole UserRole { get; set; } = null!;
    }
}
