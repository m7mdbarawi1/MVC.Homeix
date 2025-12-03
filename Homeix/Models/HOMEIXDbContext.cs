using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Homeix.Models;

public partial class HOMEIXDbContext : DbContext
{
    public HOMEIXDbContext()
    {
    }

    public HOMEIXDbContext(DbContextOptions<HOMEIXDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Advertisement> Advertisements { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

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

    public virtual DbSet<Sondo> Sondos { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<TestTable> TestTables { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<WorkerApproval> WorkerApprovals { get; set; }

    public virtual DbSet<WorkerPost> WorkerPosts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=db32386.public.databaseasp.net,1433;Database=db32386;User Id=db32386;Password=9Bm_C?2agZ+3;TrustServerCertificate=True;Encrypt=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Advertisement>(entity =>
        {
            entity.HasKey(e => e.AdId).HasName("PK__Advertis__7130D58E25DFBEB3");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Advertisements)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Advertise__Creat__10566F31");
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("PK__Conversa__C050D89792B3970A");

            entity.HasOne(d => d.User1).WithMany(p => p.ConversationUser1s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Conversat__User1__7C4F7684");

            entity.HasOne(d => d.User2).WithMany(p => p.ConversationUser2s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Conversat__User2__7D439ABD");
        });

        modelBuilder.Entity<CustomerPost>(entity =>
        {
            entity.HasKey(e => e.CustomerPostId).HasName("PK__Customer__4D7D86CAAA6D6C40");

            entity.HasOne(d => d.PostCategory).WithMany(p => p.CustomerPosts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerP__PostC__5CD6CB2B");

            entity.HasOne(d => d.User).WithMany(p => p.CustomerPosts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerP__UserI__5BE2A6F2");
        });

        modelBuilder.Entity<FavoritePost>(entity =>
        {
            entity.HasKey(e => e.FavoritePostId).HasName("PK__Favorite__5DF8E4A9B34BE76B");

            entity.HasOne(d => d.User).WithMany(p => p.FavoritePosts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FavoriteP__UserI__797309D9");
        });

        modelBuilder.Entity<JobProgress>(entity =>
        {
            entity.HasKey(e => e.JobProgressId).HasName("PK__JobProgr__860611B2C1FED980");

            entity.HasOne(d => d.AssignedToUser).WithMany(p => p.JobProgressAssignedToUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JobProgre__Assig__6B24EA82");

            entity.HasOne(d => d.CustomerPost).WithMany(p => p.JobProgresses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JobProgre__Custo__693CA210");

            entity.HasOne(d => d.RequestedByUser).WithMany(p => p.JobProgressRequestedByUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__JobProgre__Reque__6A30C649");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Message__C87C037C2ADEDA65");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Message__Convers__00200768");

            entity.HasOne(d => d.SenderUser).WithMany(p => p.Messages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Message__SenderU__01142BA1");
        });

        modelBuilder.Entity<Offer>(entity =>
        {
            entity.HasKey(e => e.OfferId).HasName("PK__Offer__8EBCF0B1C9635585");

            entity.HasOne(d => d.CustomerPost).WithMany(p => p.Offers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Offer__CustomerP__656C112C");

            entity.HasOne(d => d.User).WithMany(p => p.Offers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Offer__UserID__66603565");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A58B0273C7E");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__Payment__0D7A0286");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__Subscri__0C85DE4D");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__UserID__0B91BA14");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__PaymentM__DC31C1F368BA82AE");
        });

        modelBuilder.Entity<PostCategory>(entity =>
        {
            entity.HasKey(e => e.PostCategoryId).HasName("PK__PostCate__FE61E3692F8EC163");
        });

        modelBuilder.Entity<PostMedium>(entity =>
        {
            entity.HasKey(e => e.MediaId).HasName("PK__PostMedi__B2C2B5AF7C85BC8A");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("PK__Rating__FCCDF85C9C78C7ED");

            entity.HasOne(d => d.JobProgress).WithMany(p => p.Ratings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Rating__JobProgr__6FE99F9F");

            entity.HasOne(d => d.RatedUser).WithMany(p => p.RatingRatedUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Rating__RatedUse__71D1E811");

            entity.HasOne(d => d.RaterUser).WithMany(p => p.RatingRaterUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Rating__RaterUse__70DDC3D8");
        });

        modelBuilder.Entity<RatingCustomerPost>(entity =>
        {
            entity.HasKey(e => e.RatingCustomerPostId).HasName("PK__RatingCu__AD28CAD0F73DBC03");

            entity.HasOne(d => d.JobProgress).WithMany(p => p.RatingCustomerPosts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RatingCus__JobPr__74AE54BC");

            entity.HasOne(d => d.RatedUser).WithMany(p => p.RatingCustomerPostRatedUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RatingCus__Rated__76969D2E");

            entity.HasOne(d => d.RaterUser).WithMany(p => p.RatingCustomerPostRaterUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RatingCus__Rater__75A278F5");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__Subscrip__9A2B24BD1AD8529E");

            entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Subscript__PlanI__06CD04F7");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Subscript__UserI__05D8E0BE");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__Subscrip__755C22D7EB3B0FB3");
        });

        modelBuilder.Entity<TestTable>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PK__TestTabl__8CC33100762492FB");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC2794E0F8");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User__RoleID__534D60F1");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__UserRole__8AFACE3AFA022930");
        });

        modelBuilder.Entity<WorkerApproval>(entity =>
        {
            entity.HasKey(e => e.ApprovalId).HasName("PK__WorkerAp__328477D4438480D6");

            entity.HasOne(d => d.ReviewedByUser).WithMany(p => p.WorkerApprovalReviewedByUsers).HasConstraintName("FK__WorkerApp__Revie__571DF1D5");

            entity.HasOne(d => d.User).WithMany(p => p.WorkerApprovalUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkerApp__UserI__5629CD9C");
        });

        modelBuilder.Entity<WorkerPost>(entity =>
        {
            entity.HasKey(e => e.WorkerPostId).HasName("PK__WorkerPo__58754CB369E70C6A");

            entity.HasOne(d => d.PostCategory).WithMany(p => p.WorkerPosts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkerPos__PostC__60A75C0F");

            entity.HasOne(d => d.User).WithMany(p => p.WorkerPosts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WorkerPos__UserI__5FB337D6");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
