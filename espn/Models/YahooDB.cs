using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace espn.Models
{
    public partial class YahooDB : DbContext
    {
        public YahooDB()
        {
        }

        public YahooDB(DbContextOptions<YahooDB> options)
            : base(options)
        {
        }

        public virtual DbSet<Games> Games { get; set; }
        public virtual DbSet<GlobalParams> GlobalParams { get; set; }
        public virtual DbSet<LeagueTeams> LeagueTeams { get; set; }
        public virtual DbSet<Players> Players { get; set; }
        public virtual DbSet<YahooTeams> YahooTeams { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("DataSource=" + Path.Combine(Environment.CurrentDirectory, "espn.sqlite"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Games>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.HasIndex(e => e.GameDate)
                    .HasName("GameDateIndex");

                entity.Property(e => e.Pk)
                    .HasColumnType("int")
                    .ValueGeneratedNever();

                entity.Property(e => e.Ast).HasColumnType("double");

                entity.Property(e => e.Blk).HasColumnType("double");

                entity.Property(e => e.FgPer).HasColumnType("double");

                entity.Property(e => e.Fga).HasColumnType("double");

                entity.Property(e => e.Fgm).HasColumnType("double");

                entity.Property(e => e.FtPer).HasColumnType("double");

                entity.Property(e => e.Fta).HasColumnType("double");

                entity.Property(e => e.Ftm).HasColumnType("double");

                entity.Property(e => e.GameDate).HasColumnType("datetime");

                entity.Property(e => e.Gp).HasColumnType("int");

                entity.Property(e => e.Min).HasColumnType("double");

                entity.Property(e => e.Opp).HasColumnType("varchar(32)");

                entity.Property(e => e.Pf).HasColumnType("double");

                entity.Property(e => e.PlayerId).HasColumnType("int");

                entity.Property(e => e.Pts).HasColumnType("double");

                entity.Property(e => e.Reb).HasColumnType("double");

                entity.Property(e => e.Score).HasColumnType("double");

                entity.Property(e => e.Stl).HasColumnType("double");

                entity.Property(e => e.To).HasColumnType("double");

                entity.Property(e => e.TpPer).HasColumnType("double");

                entity.Property(e => e.Tpa).HasColumnType("double");

                entity.Property(e => e.Tpm).HasColumnType("double");
            });

            modelBuilder.Entity<GlobalParams>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.Property(e => e.Pk)
                    .HasColumnName("pk")
                    .ValueGeneratedNever();

                entity.Property(e => e.LastUpdateTime)
                    .IsRequired()
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<LeagueTeams>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.Property(e => e.Pk)
                    .HasColumnType("int")
                    .ValueGeneratedNever();

                entity.Property(e => e.Abbreviation).HasColumnType("nvarchar(32)");

                entity.Property(e => e.Name).HasColumnType("nvarchar(32)");

                entity.Property(e => e.TeamId)
                    .HasColumnName("TeamID")
                    .HasColumnType("int");
            });

            modelBuilder.Entity<Players>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasColumnType("int")
                    .ValueGeneratedNever();

                entity.Property(e => e.Age).HasColumnType("int");

                entity.Property(e => e.Misc).HasColumnType("varchar(32)");

                entity.Property(e => e.Name).HasColumnType("varchar(32)");

                entity.Property(e => e.Team).HasColumnType("varchar(32)");
            });

            modelBuilder.Entity<YahooTeams>(entity =>
            {
                entity.HasKey(e => e.Pk);

                entity.HasIndex(e => e.Pk)
                    .IsUnique();

                entity.HasIndex(e => e.TeamId)
                    .IsUnique();

                entity.Property(e => e.Pk)
                    .HasColumnName("pk")
                    .ValueGeneratedNever();

                entity.Property(e => e.TeamName).IsRequired();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
