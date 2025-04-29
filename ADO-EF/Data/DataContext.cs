using Microsoft.EntityFrameworkCore;
using ADO_EF.Data.Entities;

namespace ADO_EF.Data
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserAccess> UserAccesses { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatMember> ChatMembers { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DataContext()
        {
        }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=swift-talk;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAccess>()
                .HasIndex(ua => ua.Login)
                .IsUnique();

            modelBuilder.Entity<UserAccess>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserAccesses)
                .HasForeignKey(ua => ua.UserId)
                .HasPrincipalKey(u => u.Id);

            modelBuilder.Entity<UserAccess>()
                .HasOne(ua => ua.UserRole)
                .WithMany(ur => ur.UserAccesses)
                .HasForeignKey(ua => ua.RoleId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserAccesses)
                .WithOne(ua => ua.User)
                .HasForeignKey(ua => ua.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserImages)
                .WithOne(ui => ui.User)
                .HasForeignKey(ui => ui.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ChatMembers)
                .WithOne(cm => cm.User)
                .HasForeignKey(cm => cm.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.SentMessages)
                .WithOne(m => m.Sender)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Contacts)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ContactedBy)
                .WithOne(c => c.ContactUser)
                .HasForeignKey(c => c.ContactUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserImage>()
                .HasKey(ui => new { ui.UserId, ui.ImageUrl }); 

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Members)
                .WithOne(cm => cm.Chat)
                .HasForeignKey(cm => cm.ChatId);

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Chat)
                .HasForeignKey(m => m.ChatId);

            modelBuilder.Entity<ChatMember>()
                .HasKey(cm => cm.Id);

            modelBuilder.Entity<Message>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Contact>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Notification>()
                .HasKey(n => n.Id);

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { Id = "SelfRegistered", Name = "Self Registered User" },
                new UserRole { Id = "Admin", Name = "Administrator" }
            );
        }
    }
}