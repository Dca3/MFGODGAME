using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MafiaMMORPG.Domain.Entities;

namespace MafiaMMORPG.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerStats> PlayerStats { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemAffix> ItemAffixes { get; set; }
    public DbSet<PlayerInventory> PlayerInventories { get; set; }
    public DbSet<Quest> Quests { get; set; }
    public DbSet<PlayerQuest> PlayerQuests { get; set; }
    public DbSet<Duel> Duels { get; set; }
            public DbSet<Rating> Ratings { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<Leaderboard> Leaderboards { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Player configurations
        builder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // PlayerStats configurations
        builder.Entity<PlayerStats>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player)
                .WithOne(e => e.Stats)
                .HasForeignKey<PlayerStats>(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Item configurations
        builder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Slot).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TagsJson).HasColumnType("jsonb");
        });

        // ItemAffix configurations
        builder.Entity<ItemAffix>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.Item)
                .WithMany(e => e.Affixes)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PlayerInventory configurations
        builder.Entity<PlayerInventory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RollDataJson).HasColumnType("jsonb");
            entity.HasOne(e => e.Player)
                .WithMany(e => e.Inventory)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Item)
                .WithMany(e => e.PlayerInventories)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.PlayerId, e.ItemId }).IsUnique();
        });

        // Quest configurations
        builder.Entity<Quest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.StoryJson).HasColumnType("jsonb");
            entity.Property(e => e.RewardsJson).HasColumnType("jsonb");
            entity.Property(e => e.RequirementsJson).HasColumnType("jsonb");
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.NpcName).HasMaxLength(50);
        });

        // PlayerQuest configurations
        builder.Entity<PlayerQuest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProgressJson).HasColumnType("jsonb");
            entity.HasOne(e => e.Player)
                .WithMany(e => e.Quests)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Quest)
                .WithMany(e => e.PlayerQuests)
                .HasForeignKey(e => e.QuestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Duel configurations
        builder.Entity<Duel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActionsJson).HasColumnType("jsonb");
            entity.Property(e => e.LogJson).HasColumnType("jsonb");
            entity.Property(e => e.Seed).IsRequired();
            entity.HasOne(e => e.Player1)
                .WithMany(e => e.DuelsAsPlayer1)
                .HasForeignKey(e => e.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Player2)
                .WithMany(e => e.DuelsAsPlayer2)
                .HasForeignKey(e => e.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Winner)
                .WithMany()
                .HasForeignKey(e => e.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

                            // Rating configurations
                    builder.Entity<Rating>(entity =>
                    {
                        entity.HasKey(e => e.Id);
                        entity.HasOne(e => e.Player)
                            .WithOne(e => e.Rating)
                            .HasForeignKey<Rating>(e => e.PlayerId)
                            .OnDelete(DeleteBehavior.Cascade);
                        entity.HasIndex(e => e.PlayerId).IsUnique();
                        entity.HasIndex(e => e.MMR).IsDescending(); // Leaderboard için
                    });

                    // RefreshToken configurations
                    builder.Entity<RefreshToken>(entity =>
                    {
                        entity.HasKey(e => e.Id);
                        entity.HasIndex(e => e.UserId);
                        entity.HasIndex(e => e.Token).IsUnique();
                    });

        // Season configurations
        builder.Entity<Season>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.RewardsJson).HasColumnType("jsonb");
        });

        // Leaderboard configurations
        builder.Entity<Leaderboard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Season)
                .WithMany(e => e.Leaderboards)
                .HasForeignKey(e => e.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Player)
                .WithMany(e => e.Leaderboards)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.SeasonId, e.PlayerId }).IsUnique();
        });
    }
}
