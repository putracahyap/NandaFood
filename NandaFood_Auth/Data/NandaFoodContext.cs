using Microsoft.EntityFrameworkCore;
using NandaFood_Auth.Models;

namespace NandaFood_Auth.Data;

public partial class NandafoodContext : DbContext
{
    public NandafoodContext()
    {
    }

    public NandafoodContext(DbContextOptions<NandafoodContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    
    public virtual DbSet<RevokedToken> RevokedTokens { get; set; }
    
    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("last_name");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("updated_date");
            entity.Property(e => e.UserRole)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("user_role");
            entity.Property(e => e.UserSecret)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("user_secret");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");
            entity.Property(e => e.JwtToken)
                .IsUnicode(false)
                .HasColumnName("jwt_token");
            
            entity.HasOne(d => d.UserRoleNavigation).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.UserRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accounts_Roles");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshToken");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.AccountsId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("accounts_id");
            entity.Property(e => e.DateAdded)
                .HasColumnType("datetime")
                .HasColumnName("date_added");
            entity.Property(e => e.DateExpire)
                .HasColumnType("datetime")
                .HasColumnName("date_expire");
            entity.Property(e => e.IsRevoked).HasColumnName("is_revoked");
            entity.Property(e => e.JwtId)
                .IsUnicode(false)
                .HasColumnName("jwt_id");
            entity.Property(e => e.Token)
                .IsUnicode(false)
                .HasColumnName("token");

            entity.HasOne(d => d.Accounts).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.AccountsId)
                .HasConstraintName("FK_RefreshToken_Accounts");
        });
        
        modelBuilder.Entity<RevokedToken>(entity =>
        {
            entity.ToTable("RevokedToken");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.RevocationDate)
                .HasColumnType("datetime")
                .HasColumnName("revocation_date");
            entity.Property(e => e.Token)
                .IsUnicode(false)
                .HasColumnName("token");
        });
        
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleCode);

            entity.Property(e => e.RoleCode)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("role_code");
            entity.Property(e => e.Description)
                .HasMaxLength(20)
                .HasColumnName("description");
        });
    }
}


