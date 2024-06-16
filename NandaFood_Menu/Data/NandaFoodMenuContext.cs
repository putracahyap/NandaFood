using Microsoft.EntityFrameworkCore;
using NandaFood_Menu.Models;

namespace NandaFood_Menu.Data;

public class NandaFoodMenuContext : DbContext
{
    public NandaFoodMenuContext()
    {
    }

    public NandaFoodMenuContext(DbContextOptions<NandaFoodMenuContext> options)
        : base(options)
    {
    }
    
    public virtual DbSet<FoodMenu> FoodMenu { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FoodMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Menu");

            entity.ToTable("FoodMenu");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("created_by");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("created_date");
            entity.Property(e => e.Menu)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("menu");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("updated_by");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("updated_date");
        });
    }
}