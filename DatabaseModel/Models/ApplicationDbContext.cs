using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DatabaseModel.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Salesperson> Salespersons { get; set; }

    public virtual DbSet<Showroom> Showrooms { get; set; }

    public virtual DbSet<Visitor> Visitors { get; set; }
    

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Showroom;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__admins__3213E83FD7400FA3");

            entity.ToTable("admins");

            entity.HasIndex(e => e.Email, "UQ__admins__AB6E6164ACC71181").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__conversa__3213E83F45A250DF");

            entity.ToTable("conversations");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("created_at");
            entity.Property(e => e.Title).HasColumnName("title");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__feedback__3213E83F0CFB4660");

            entity.ToTable("feedback");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("created_at");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ShowroomId).HasColumnName("showroom_id");
            entity.Property(e => e.VisitorId).HasColumnName("visitor_id");

            entity.HasOne(d => d.Showroom).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.ShowroomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__feedback__showro__797309D9");

            entity.HasOne(d => d.Visitor).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.VisitorId)
                .HasConstraintName("FK__feedback__visito__787EE5A0");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__messages__3213E83FE3D9BC92");

            entity.ToTable("messages");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("created_at");
            entity.Property(e => e.Role).HasColumnName("role");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("FK__messages__conver__02FC7413");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__notifica__3213E83F37B352C3");

            entity.ToTable("notifications");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Type)
                .HasDefaultValue("new_visitor")
                .HasColumnName("type");
            entity.Property(e => e.VisitorId).HasColumnName("visitor_id");

            entity.HasOne(d => d.Visitor).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.VisitorId)
                .HasConstraintName("FK__notificat__visit__7F2BE32F");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__products__3213E83FCDCD56FD");

            entity.ToTable("products");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.NameAr).HasColumnName("name_ar");
        });

        modelBuilder.Entity<Salesperson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__salesper__3213E83F7032FF32");

            entity.ToTable("salespersons");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.ShowroomId).HasColumnName("showroom_id");

            entity.HasOne(d => d.Showroom).WithMany(p => p.Salespeople)
                .HasForeignKey(d => d.ShowroomId)
                .HasConstraintName("FK__salespers__showr__6A30C649");
        });

        modelBuilder.Entity<Showroom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__showroom__3213E83F53ABF816");

            entity.ToTable("showrooms");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("created_at");
            entity.Property(e => e.Location).HasColumnName("location");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.NameAr).HasColumnName("name_ar");
            entity.Property(e => e.Phone).HasColumnName("phone");
        });

        modelBuilder.Entity<Visitor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__visitors__3213E83FA75E536F");

            entity.ToTable("visitors");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cpr).HasColumnName("cpr");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FollowUpNotes).HasColumnName("follow_up_notes");
            entity.Property(e => e.FollowUpStatus)
                .HasDefaultValue("pending")
                .HasColumnName("follow_up_status");
            entity.Property(e => e.IsInterested).HasColumnName("is_interested");
            entity.Property(e => e.IsReturning)
                .HasDefaultValue(false)
                .HasColumnName("is_returning");
            entity.Property(e => e.Language)
                .HasDefaultValue("en")
                .HasColumnName("language");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Phone).HasColumnName("phone");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SalespersonId).HasColumnName("salesperson_id");
            entity.Property(e => e.Score)
                .HasDefaultValue(0)
                .HasColumnName("score");
            entity.Property(e => e.ShowroomId).HasColumnName("showroom_id");
            entity.Property(e => e.VisitedAt)
                .HasDefaultValueSql("(sysdatetimeoffset())")
                .HasColumnName("visited_at");

            entity.HasOne(d => d.Product).WithMany(p => p.Visitors)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__visitors__produc__73BA3083");

            entity.HasOne(d => d.Salesperson).WithMany(p => p.Visitors)
                .HasForeignKey(d => d.SalespersonId)
                .HasConstraintName("FK__visitors__salesp__74AE54BC");

            entity.HasOne(d => d.Showroom).WithMany(p => p.Visitors)
                .HasForeignKey(d => d.ShowroomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__visitors__showro__72C60C4A");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
