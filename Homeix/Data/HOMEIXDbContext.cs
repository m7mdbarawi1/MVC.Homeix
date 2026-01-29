using Homeix.Models;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Data
{
    public partial class HOMEIXDbContext : DbContext
    {
        public HOMEIXDbContext() { }

        public HOMEIXDbContext(DbContextOptions<HOMEIXDbContext> options)
            : base(options) { }

        public virtual DbSet<Advertisement> Advertisements { get; set; } = null!;
        public virtual DbSet<Conversation> Conversations { get; set; } = null!;
        public virtual DbSet<CustomerPost> CustomerPosts { get; set; } = null!;
        public virtual DbSet<FavoriteWorkerPost> FavoriteWorkerPosts { get; set; } = null!;
        public virtual DbSet<FavoriteCustomerPost> FavoriteCustomerPosts { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;
        public virtual DbSet<Payment> Payments { get; set; } = null!;
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;
        public virtual DbSet<PostCategory> PostCategories { get; set; } = null!;
        public virtual DbSet<Rating> Ratings { get; set; } = null!;
        public virtual DbSet<Subscription> Subscriptions { get; set; } = null!;
        public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserRole> UserRoles { get; set; } = null!;
        public virtual DbSet<WorkerApproval> WorkerApprovals { get; set; } = null!;
        public virtual DbSet<WorkerPost> WorkerPosts { get; set; } = null!;
        public virtual DbSet<WorkerPostMedia> WorkerPostMedia { get; set; } = null!;
        public virtual DbSet<CustomerPostMedia> CustomerPostMedia { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=db32386.public.databaseasp.net,1433;Database=db32386;User Id=db32386;Password=9Bm_C?2agZ+3;TrustServerCertificate=True;Encrypt=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Advertisement>(entity =>
            {
                entity.HasKey(e => e.AdId);
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany(u => u.Advertisements)
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(e => e.ConversationId);

                entity.HasOne(e => e.User1)
                      .WithMany(u => u.ConversationUser1s)
                      .HasForeignKey(e => e.User1Id)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.User2)
                      .WithMany(u => u.ConversationUser2s)
                      .HasForeignKey(e => e.User2Id)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CustomerPost>(entity =>
            {
                entity.HasKey(e => e.CustomerPostId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.CustomerPosts)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.PostCategory)
                      .WithMany(c => c.CustomerPosts)
                      .HasForeignKey(e => e.PostCategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ✅ Favorites: Worker Posts
            modelBuilder.Entity<FavoriteWorkerPost>(entity =>
            {
                entity.HasKey(e => e.FavoriteWorkerPostId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.FavoriteWorkerPosts)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // ✅ Recommended: enforce integrity with WorkerPost table
                entity.HasOne<WorkerPost>()
                      .WithMany()
                      .HasForeignKey(e => e.WorkerPostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.WorkerPostId })
                      .IsUnique();
            });

            // ✅ Favorites: Customer Posts
            modelBuilder.Entity<FavoriteCustomerPost>(entity =>
            {
                entity.HasKey(e => e.FavoriteCustomerPostId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.FavoriteCustomerPosts)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // ✅ Recommended: enforce integrity with CustomerPost table
                entity.HasOne<CustomerPost>()
                      .WithMany()
                      .HasForeignKey(e => e.CustomerPostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.CustomerPostId })
                      .IsUnique();
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.MessageId);

                entity.HasOne(e => e.Conversation)
                      .WithMany(c => c.Messages)
                      .HasForeignKey(e => e.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.SenderUser)
                      .WithMany(u => u.Messages)
                      .HasForeignKey(e => e.SenderUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Payments)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Subscription)
                      .WithMany(s => s.Payments)
                      .HasForeignKey(e => e.SubscriptionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PaymentMethod)
                      .WithMany(m => m.Payments)
                      .HasForeignKey(e => e.PaymentMethodId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(e => e.RatingId);

                entity.HasOne(e => e.RaterUser)
                      .WithMany(u => u.RatingRaterUsers)
                      .HasForeignKey(e => e.RaterUserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.RatedUser)
                      .WithMany(u => u.RatingRatedUsers)
                      .HasForeignKey(e => e.RatedUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.SubscriptionId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Subscriptions)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Plan)
                      .WithMany(p => p.Subscriptions)
                      .HasForeignKey(e => e.PlanId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WorkerApproval>(entity =>
            {
                entity.HasKey(e => e.ApprovalId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.WorkerApprovalUsers)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<WorkerPost>(entity =>
            {
                entity.HasKey(e => e.WorkerPostId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.WorkerPosts)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.PostCategory)
                      .WithMany(c => c.WorkerPosts)
                      .HasForeignKey(e => e.PostCategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.HasOne(e => e.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<WorkerPostMedia>(entity =>
            {
                entity.HasOne(e => e.WorkerPost)
                      .WithMany(p => p.Media)
                      .HasForeignKey(e => e.WorkerPostId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CustomerPostMedia>(entity =>
            {
                entity.HasOne(e => e.CustomerPost)
                      .WithMany(p => p.Media)
                      .HasForeignKey(e => e.CustomerPostId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
