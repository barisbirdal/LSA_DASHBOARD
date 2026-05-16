using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LSA_Dashboard.Models;

public partial class LsaDbContext : DbContext
{
    public LsaDbContext()
    {
    }

    public LsaDbContext(DbContextOptions<LsaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CrmClientDetail> CrmClientDetails { get; set; }

    public virtual DbSet<CrmContract> CrmContracts { get; set; }

    public virtual DbSet<CrmInvoice> CrmInvoices { get; set; }

    public virtual DbSet<CrmSector> CrmSectors { get; set; }

    public virtual DbSet<CrmSegment> CrmSegments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=LSA_Dashboard;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CrmClientDetail>(entity =>
        {
            entity.HasKey(e => e.OrganizationId).HasName("PK__CRM_Clie__CADB0B1297D915EB");

            entity.ToTable("CRM_ClientDetails");

            entity.Property(e => e.OrganizationId).ValueGeneratedNever();
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Region).HasMaxLength(100);

            entity.HasOne(d => d.Sector).WithMany(p => p.CrmClientDetails)
                .HasForeignKey(d => d.SectorId)
                .HasConstraintName("FK__CRM_Clien__Secto__3B75D760");

            entity.HasOne(d => d.Segment).WithMany(p => p.CrmClientDetails)
                .HasForeignKey(d => d.SegmentId)
                .HasConstraintName("FK__CRM_Clien__Segme__3C69FB99");
        });

        modelBuilder.Entity<CrmContract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CRM_Cont__3214EC0743DF02BF");

            entity.ToTable("CRM_Contracts");

            entity.Property(e => e.AutoRenew).HasDefaultValue(true);
            entity.Property(e => e.ContractType).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);

            entity.HasOne(d => d.Organization).WithMany(p => p.CrmContracts)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("FK__CRM_Contr__Organ__4222D4EF");
        });

        modelBuilder.Entity<CrmInvoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CRM_Invo__3214EC070F24F12D");

            entity.ToTable("CRM_Invoices");

            entity.Property(e => e.FixedFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.InvoiceDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentStatus).HasMaxLength(20);
            entity.Property(e => e.TotalAmount)
                .HasComputedColumnSql("([FixedFee]+[UsageBasedFee])", false)
                .HasColumnType("decimal(19, 2)");
            entity.Property(e => e.UsageBasedFee).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Contract).WithMany(p => p.CrmInvoices)
                .HasForeignKey(d => d.ContractId)
                .HasConstraintName("FK__CRM_Invoi__Contr__45F365D3");
        });

        modelBuilder.Entity<CrmSector>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CRM_Sect__3214EC07F8241B09");

            entity.ToTable("CRM_Sectors");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<CrmSegment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CRM_Segm__3214EC07EC352A25");

            entity.ToTable("CRM_Segments");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
