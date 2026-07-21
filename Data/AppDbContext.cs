using Microsoft.EntityFrameworkCore;
using Backend_Mboagreen.Models;

namespace Backend_Mboagreen.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<AnnonceModel> Annonce { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<VisitModel> Visits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AnnonceModel>()
                .HasOne(a => a.User)
                .WithMany(u => u.Annonce)
                .HasForeignKey(a => a.Users_Id);
        }
    }
}