using Homeix.Models;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Data
{
    public partial class HOMEIXDbContext : DbContext
    {
        public HOMEIXDbContext() { }
        public HOMEIXDbContext(DbContextOptions<HOMEIXDbContext> options) : base(options) { }

        public virtual DbSet<Advertisement> Advertisements { get; set; }
        public virtual DbSet<Conversation> Conversations { get; set; }
        public virtual DbSet<CustomerPost> CustomerPosts { get; set; }
        public virtual DbSet<FavoritePost> FavoritePosts { get; set; }
        public virtual DbSet<JobProgress> JobProgresses { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Offer> Offers { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
        public virtual DbSet<PostCategory> PostCategories { get; set; }
        public virtual DbSet<PostMedium> PostMedia { get; set; }
        public virtual DbSet<Rating> Ratings { get; set; }
        public virtual DbSet<RatingCustomerPost> RatingCustomerPosts { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<WorkerApproval> WorkerApprovals { get; set; }
        public virtual DbSet<WorkerPost> WorkerPosts { get; set; }

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
            // Advertisement
            modelBuilder.Entity<Advertisement>(entity =>
            {
                entity.HasKey(e => e.AdId);
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany(u => u.Advertisements)
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // Conversation
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(e => e.ConversationId);
                entity.HasOne(e => e.User1)
                      .WithMany(u => u.ConversationUser1s)
                      .HasForeignKey(e => e.User1Id)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.User2)
                      .WithMany(u => u.ConversationUser2s)
                      .HasForeignKey(e => e.User2Id)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // CustomerPost
            modelBuilder.Entity<CustomerPost>(entity =>
            {
                entity.HasKey(e => e.CustomerPostId);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.CustomerPosts)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.PostCategory)
                      .WithMany(c => c.CustomerPosts)
                      .HasForeignKey(e => e.PostCategoryId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // FavoritePost
            modelBuilder.Entity<FavoritePost>(entity =>
            {
                entity.HasKey(e => e.FavoritePostId);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.FavoritePosts)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // JobProgress
            modelBuilder.Entity<JobProgress>(entity =>
            {
                entity.HasKey(e => e.JobProgressId);

                entity.HasOne(e => e.CustomerPost)
                      .WithMany(c => c.JobProgresses)
                      .HasForeignKey(e => e.CustomerPostId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.RequestedByUser)
                      .WithMany(u => u.JobProgressRequestedByUsers)
                      .HasForeignKey(e => e.RequestedByUserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.AssignedToUser)
                      .WithMany(u => u.JobProgressAssignedToUsers)
                      .HasForeignKey(e => e.AssignedToUserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // Message
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.MessageId);

                entity.HasOne(e => e.Conversation)
                      .WithMany(c => c.Messages)
                      .HasForeignKey(e => e.ConversationId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.SenderUser)
                      .WithMany(u => u.Messages)
                      .HasForeignKey(e => e.SenderUserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // Offer
            modelBuilder.Entity<Offer>(entity =>
            {
                entity.HasKey(e => e.OfferId);

                entity.HasOne(e => e.CustomerPost)
                      .WithMany(c => c.Offers)
                      .HasForeignKey(e => e.CustomerPostId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Offers)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Payments)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.Subscription)
                      .WithMany(s => s.Payments)
                      .HasForeignKey(e => e.SubscriptionId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.PaymentMethod)
                      .WithMany(m => m.Payments)
                      .HasForeignKey(e => e.PaymentMethodId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // Rating
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(e => e.RatingId);

                entity.HasOne(e => e.JobProgress)
                      .WithMany(j => j.Ratings)
                      .HasForeignKey(e => e.JobProgressId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.RatedUser)
                      .WithMany(u => u.RatingRatedUsers)
                      .HasForeignKey(e => e.RatedUserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.RaterUser)
                      .WithMany(u => u.RatingRaterUsers)
                      .HasForeignKey(e => e.RaterUserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // RatingCustomerPost
            modelBuilder.Entity<RatingCustomerPost>(entity =>
            {
                entity.HasKey(e => e.RatingCustomerPostId);

                entity.HasOne(e => e.JobProgress)
                      .WithMany(j => j.RatingCustomerPosts)
                      .HasForeignKey(e => e.JobProgressId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.RatedUser)
                      .WithMany(u => u.RatingCustomerPostRatedUsers)
                      .HasForeignKey(e => e.RatedUserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.RaterUser)
                      .WithMany(u => u.RatingCustomerPostRaterUsers)
                      .HasForeignKey(e => e.RaterUserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // Subscription
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.SubscriptionId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Subscriptions)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.Plan)
                      .WithMany(p => p.Subscriptions)
                      .HasForeignKey(e => e.PlanId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // WorkerApproval
            modelBuilder.Entity<WorkerApproval>(entity =>
            {
                entity.HasKey(e => e.ApprovalId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.WorkerApprovalUsers)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.ReviewedByUser)
                      .WithMany(u => u.WorkerApprovalReviewedByUsers)
                      .HasForeignKey(e => e.ReviewedByUserId);
            });

            // WorkerPost
            modelBuilder.Entity<WorkerPost>(entity =>
            {
                entity.HasKey(e => e.WorkerPostId);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.WorkerPosts)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(e => e.PostCategory)
                      .WithMany(c => c.WorkerPosts)
                      .HasForeignKey(e => e.PostCategoryId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.HasOne(e => e.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.ClientSetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
