using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProductManagement.EFCore.IdentityModels;

namespace ProductManagement.EFCore.Models;

public partial class ProductManagementDBContext : IdentityDbContext<ApplicationUser>
{
    public ProductManagementDBContext(DbContextOptions<ProductManagementDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Nationality> Nationalities { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<UsersOTP> UsersOTPs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Language__3214EC27C16B2350");
        });

        modelBuilder.Entity<Nationality>(entity =>
        {
            entity.Property(e => e.Name).UseCollation("Arabic_CI_AS");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC07BA18DC90");
        });

        modelBuilder.Entity<UsersOTP>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserOTP__3214EC072D0F7497");
        });

        OnModelCreatingPartial(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
