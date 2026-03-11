using System;
using System.Collections.Generic;
using DraftService.Models;
using Microsoft.EntityFrameworkCore;

namespace DraftService.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Draft> Drafts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Draft>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Drafts__3214EC07C6AFE956");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Continent).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
