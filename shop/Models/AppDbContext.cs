using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace shop.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Manufacturer> Manufacturers { get; set; }

    public virtual DbSet<OrderItemsImport> OrderItemsImports { get; set; }

    public virtual DbSet<OrdersImport> OrdersImports { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductsImport> ProductsImports { get; set; }

    public virtual DbSet<ProductsSubcategory> ProductsSubcategories { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SizesImport> SizesImports { get; set; }

    public virtual DbSet<StockItemsImport> StockItemsImports { get; set; }

    public virtual DbSet<UsersImport> UsersImports { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=shop_s;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Cyrillic_General_100_CI_AI_KS_SC_UTF8");

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.ToTable("Manufacturer$");

            entity.Property(e => e.ManufacturerId)
                .ValueGeneratedNever()
                .HasColumnName("ManufacturerID");
            entity.Property(e => e.Manufacturer1)
                .HasMaxLength(255)
                .HasColumnName("Manufacturer");
        });

        modelBuilder.Entity<OrderItemsImport>(entity =>
        {
            entity.HasKey(e => e.OrderItemId);

            entity.ToTable("Order_Items_import$");

            entity.Property(e => e.OrderItemId)
                .ValueGeneratedNever()
                .HasColumnName("OrderItemID");
            entity.Property(e => e.Price).HasColumnType("money");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.SizeId).HasColumnName("SizeID");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItemsImports)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Items_import$_Products_import$");

            entity.HasOne(d => d.Size).WithMany(p => p.OrderItemsImports)
                .HasForeignKey(d => d.SizeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Items_import$_Sizes_import$");
        });

        modelBuilder.Entity<OrdersImport>(entity =>
        {
            entity.HasKey(e => e.OrderNumber).HasName("PK_Orders_import");

            entity.ToTable("Orders_import$");

            entity.Property(e => e.OrderNumber).ValueGeneratedNever();
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.OrderNumberNavigation).WithOne(p => p.OrdersImport)
                .HasForeignKey<OrdersImport>(d => d.OrderNumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_import$_Order_Items_import$");

            entity.HasOne(d => d.User).WithMany(p => p.OrdersImports)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_import$_Users_import$");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId);

            entity.ToTable("Product_category$");

            entity.Property(e => e.CategoryId)
                .ValueGeneratedNever()
                .HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(255);
        });

        modelBuilder.Entity<ProductsImport>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.ToTable("Products_import$");

            entity.Property(e => e.ProductId)
                .ValueGeneratedNever()
                .HasColumnName("ProductID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ManufacturerId).HasColumnName("ManufacturerID");
            entity.Property(e => e.Picture).HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("money");
            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.Structure).HasMaxLength(255);
            entity.Property(e => e.SubcategoryId).HasColumnName("SubcategoryID");

            entity.HasOne(d => d.Category).WithMany(p => p.ProductsImports)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_import$_Product_category$");

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.ProductsImports)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_import$_Manufacturer$");

            entity.HasOne(d => d.Subcategory).WithMany(p => p.ProductsImports)
                .HasForeignKey(d => d.SubcategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_import$_Products_subcategory$");
        });

        modelBuilder.Entity<ProductsSubcategory>(entity =>
        {
            entity.HasKey(e => e.SubcategoryId);

            entity.ToTable("Products_subcategory$");

            entity.Property(e => e.SubcategoryId)
                .ValueGeneratedNever()
                .HasColumnName("SubcategoryID");
            entity.Property(e => e.SubcategoryName).HasMaxLength(255);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles$");

            entity.Property(e => e.RoleId)
                .ValueGeneratedNever()
                .HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(255);
        });

        modelBuilder.Entity<SizesImport>(entity =>
        {
            entity.HasKey(e => e.SizeId);

            entity.ToTable("Sizes_import$");

            entity.Property(e => e.SizeId)
                .ValueGeneratedNever()
                .HasColumnName("SizeID");
        });

        modelBuilder.Entity<StockItemsImport>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.SizeId });

            entity.ToTable("Stock_Items_import$");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.SizeId).HasColumnName("SizeID");

            entity.HasOne(d => d.Size).WithMany(p => p.StockItemsImports)
                .HasForeignKey(d => d.SizeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stock_Items_import$_Sizes_import$");
        });

        modelBuilder.Entity<UsersImport>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("Users_import$");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.LastName).HasMaxLength(255);
            entity.Property(e => e.Login).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Surname).HasMaxLength(255);

            entity.HasOne(d => d.Role).WithMany(p => p.UsersImports)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_import$_Roles$");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
