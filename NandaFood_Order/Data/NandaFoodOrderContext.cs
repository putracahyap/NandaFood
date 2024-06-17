using Microsoft.EntityFrameworkCore;
using NandaFood_Order.Models;

namespace NandaFood_Order.Data;

public class NandaFoodOrderContext : DbContext
{
    public NandaFoodOrderContext()
    {
    }

    public NandaFoodOrderContext(DbContextOptions<NandaFoodOrderContext> options)
        : base(options)
    {
    }
    
    public virtual DbSet<FoodOrder> FoodOrders { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FoodOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Order");

            entity.ToTable("FoodOrder");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.Menu)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("menu");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.OrderBy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("order_by");
            entity.Property(e => e.OrderDate)
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("order_status");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price");
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