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
    
    public virtual DbSet<Menu> Menus { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Menu>(entity =>
        {
            entity.ToTable("Menu");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("id");
            entity.Property(e => e.Menu1)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("menu");
        });
    }
}