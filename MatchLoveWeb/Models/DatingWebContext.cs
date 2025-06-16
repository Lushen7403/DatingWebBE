using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MatchLoveWeb.Models;

public partial class DatingWebContext : DbContext
{
    public DatingWebContext()
    {
    }

    public DatingWebContext(DbContextOptions<DatingWebContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Block> Blocks { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Match> Matches { get; set; }

    public virtual DbSet<MatchConditon> MatchConditons { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<MessageMedium> MessageMedia { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationType> NotificationTypes { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Profile> Profiles { get; set; }

    public virtual DbSet<ProfileImage> ProfileImages { get; set; }

    public virtual DbSet<RechargePackage> RechargePackages { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<ReportType> ReportTypes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Swipe> Swipes { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }
    public virtual DbSet<Hobby> Hobbies { get; set; }
    public virtual DbSet<UserHobby> UserHobbies { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("workstation id=DatingWeb.mssql.somee.com;packet size=4096;user id=lushen_SQLLogin_1;pwd=ghueci1zxf;data source=DatingWeb.mssql.somee.com;persist security info=False;initial catalog=DatingWeb;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC07922CABF9");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Email, "UQ__Account__A9D10534AED869A5").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__Account__C9F2845627534B67").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DiamondCount).HasDefaultValueSql("((100))");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.IsBanned)
                .HasDefaultValueSql("((0))")
                .HasColumnName("isBanned");
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
            entity.Property(e => e.UserName).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Account_Role");
        });

        modelBuilder.Entity<Block>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Block__3214EC0776B7109E");

            entity.ToTable("Block");

            entity.HasIndex(e => new { e.BlockerId, e.BlockedUserId }, "UQ_Block").IsUnique();

            entity.Property(e => e.BlockAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.BlockedUser).WithMany(p => p.BlockBlockedUsers)
                .HasForeignKey(d => d.BlockedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Block_BlockedUser");

            entity.HasOne(d => d.Blocker).WithMany(p => p.BlockBlockers)
                .HasForeignKey(d => d.BlockerId)
                .HasConstraintName("FK_Block_Blocker");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Conversa__3214EC07E1291E9F");

            entity.ToTable("Conversation");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User1).WithMany(p => p.ConversationUser1s)
                .HasForeignKey(d => d.User1Id)
                .HasConstraintName("FK_Conversation_User1");

            entity.HasOne(d => d.User2).WithMany(p => p.ConversationUser2s)
                .HasForeignKey(d => d.User2Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conversation_User2");
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Gender__3214EC07F9ECABFB");

            entity.ToTable("Gender");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.GenderName).HasMaxLength(20);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3214EC0720C2ECBE");

            entity.ToTable("Location");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Account).WithMany(p => p.Locations)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK_Location_Account");
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Match__3214EC0765BB1060");

            entity.ToTable("Match");

            entity.HasIndex(e => new { e.User1Id, e.User2Id }, "UQ_Match").IsUnique();

            entity.Property(e => e.MatchedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User1).WithMany(p => p.MatchUser1s)
                .HasForeignKey(d => d.User1Id)
                .HasConstraintName("FK_Match_User1");

            entity.HasOne(d => d.User2).WithMany(p => p.MatchUser2s)
                .HasForeignKey(d => d.User2Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Match_User2");
        });

        modelBuilder.Entity<MatchConditon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MatchCon__3BD019894E67DAF0");

            entity.ToTable("MatchConditon");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Account).WithMany(p => p.MatchConditons)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TieuChi_Account");

            entity.HasOne(d => d.Gender).WithMany(p => p.MatchConditons)
                .HasForeignKey(d => d.GenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                 .IsRequired(false)
                .HasConstraintName("FK_MatchCondition_Gender");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Message__3214EC07DFCF3BF1");

            entity.ToTable("Message");

            entity.Property(e => e.IsRead).HasDefaultValueSql("((0))");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("FK_Message_Conversation");

            entity.Property(e => e.ToxicLevel)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ToxicLevel");

            entity.Property(e => e.ToxicTokenCount)
                .HasColumnName("ToxicTokenCount");

            entity.Property(e => e.ToxicDensity)
                .HasColumnType("float")
                .HasColumnName("ToxicDensity");

            entity.Property(e => e.HasHeavyWord)
                .HasColumnName("HasHeavyWord");

        });

        modelBuilder.Entity<MessageMedium>(entity =>
        {
            entity.ToTable("MessageMedia");
            entity.HasKey(e => e.Id).HasName("PK__MessageM__3214EC07844601C5");

            entity.Property(e => e.MediaType).HasMaxLength(50);
            entity.Property(e => e.MediaUrl).HasMaxLength(255);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.PublicId)
                 .HasMaxLength(255)        
                 .HasColumnName("PublicId");

            entity.HasOne(d => d.Message).WithMany(p => p.MessageMedia)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK_MessageMedia_Message");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12B5FDFCF3");

            entity.ToTable("Notification");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValueSql("((0))");

            entity.HasOne(d => d.NotificationType).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.NotificationTypeId)
                .HasConstraintName("FK_Notification_NotificationType");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Notification_User");
        });

        modelBuilder.Entity<NotificationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC071794B215");

            entity.ToTable("NotificationType");

            entity.Property(e => e.NotificationTypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payment__3214EC076CEAB9ED");

            entity.ToTable("Payment");

            entity.Property(e => e.AmountAfter).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AmountBefore).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.VnpTxnRef)
                .HasMaxLength(50)
                .HasColumnName("VnpTxnRef");

            entity.Property(e => e.VnpResponseCode)
                .HasMaxLength(10)
                .HasColumnName("VnpResponseCode");

            entity.Property(e => e.VnpTransactionStatus)
                .HasMaxLength(10)
                .HasColumnName("VnpTransactionStatus");

            entity.Property(e => e.VnpBankCode)
                .HasMaxLength(20)
                .HasColumnName("VnpBankCode");

            entity.Property(e => e.VnpBankTranNo)
                .HasMaxLength(50)
                .HasColumnName("VnpBankTranNo");

            entity.Property(e => e.VnpPayDate)
                .HasColumnType("datetime")
                .HasColumnName("VnpPayDate");

            entity.Property(e => e.VnpSecureHash)
                .HasMaxLength(128)
                .HasColumnName("VnpSecureHash");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("UpdatedAt");

            entity.HasOne(d => d.Account).WithMany(p => p.Payments)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Account");

            entity.HasOne(d => d.Package).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Package");

            entity.HasOne(d => d.Voucher).WithMany(p => p.Payments)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("FK_Payment_Voucher");
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Profile__3214EC077AD54CAD");

            entity.ToTable("Profile");

            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.Birthday).HasColumnType("date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PublicId)              
                                  .HasMaxLength(200)
                                  .HasColumnName("PublicId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Account).WithMany(p => p.Profiles)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK_Profile_TaiKhoan");

            entity.HasOne(d => d.Gender).WithMany(p => p.Profiles)
                .HasForeignKey(d => d.GenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Profile_Gender");

        });

        modelBuilder.Entity<ProfileImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProfileI__3214EC07756E78CF");

            entity.ToTable("ProfileImage");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.PublicId)
                                  .HasMaxLength(200)
                                  .HasColumnName("PublicId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Profile).WithMany(p => p.ProfileImages)
                .HasForeignKey(d => d.ProfileId)
                .HasConstraintName("FK_ProfileImage_Profile");
        });

        modelBuilder.Entity<RechargePackage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Recharge__3214EC078C51A196");

            entity.ToTable("RechargePackage");

            entity.Property(e => e.Description).HasMaxLength(200);

            entity.Property(e => e.IsActivate)
                   .HasColumnName("isActivate")
                   .IsRequired()
                   .HasDefaultValueSql("((1))");
         });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Report__3214EC078B91A43E");

            entity.ToTable("Report");

            entity.Property(e => e.IsChecked).HasDefaultValueSql("((0))");
            entity.Property(e => e.ReportAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.ReportedType).WithMany(p => p.Reports)
                .HasForeignKey(d => d.ReportedTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Report_ReportedType");

            entity.HasOne(d => d.ReportedUser).WithMany(p => p.ReportReportedUsers)
                .HasForeignKey(d => d.ReportedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Report_ReportedUser");

            entity.HasOne(d => d.User).WithMany(p => p.ReportUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Report_User");
        });

        modelBuilder.Entity<ReportType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReportTy__3214EC07113F8C4D");

            entity.ToTable("ReportType");

            entity.Property(e => e.ReportTypeName).HasMaxLength(200);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1A6A6EF3E7");

            entity.ToTable("Role");

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Swipe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Swipe__3214EC07482C8115");

            entity.ToTable("Swipe");

            entity.HasIndex(e => new { e.AccountId, e.SwipedAccountId }, "UQ_Swipe").IsUnique();

            entity.Property(e => e.SwipedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.SwipeAccounts)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK_Swipe_Account");

            entity.HasOne(d => d.SwipedAccount).WithMany(p => p.SwipeSwipedAccounts)
                .HasForeignKey(d => d.SwipedAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Swipe_SwipedAccount");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Voucher__3214EC074AF5BFA9");

            entity.ToTable("Voucher");

            entity.HasIndex(e => e.Code, "UQ__Voucher__A25C5AA7476320A5").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("((1))");
        });


        modelBuilder.Entity<UserHobby>(entity =>
        {
            entity.ToTable("UserHobby"); 

            entity.HasOne(uh => uh.Profile)
                  .WithMany(p => p.UserHobbies)
                  .HasForeignKey(uh => uh.ProfileId);
        });

        modelBuilder.Entity<Hobby>(entity =>
        {
            entity.ToTable("Hobby");

            entity.HasMany(h => h.UserHobbies)
                  .WithOne(uh => uh.Hobby)
                  .HasForeignKey(uh => uh.HobbyId);
        });



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
