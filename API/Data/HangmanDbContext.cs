using HangmanApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HangmanApi.Data
{
    public class HangmanDbContext : DbContext
    {
        public HangmanDbContext(DbContextOptions<HangmanDbContext> options) : base(options)
        {
        }

        public DbSet<Word> Words { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<GameState> GameStates { get; set; }
        public DbSet<ProfilePicture> ProfilePictures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration pour l'entité Word
            modelBuilder.Entity<Word>(entity =>
            {
                entity.ToTable("mots");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Mot).IsRequired().HasColumnName("mot");
            });

            // Configuration pour l'entité User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserName).IsRequired().HasColumnName("username");
                entity.Property(e => e.Password).IsRequired().HasColumnName("password");

                entity.HasMany(e => e.Scores)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .HasConstraintName("fk_user");

                entity.HasOne(e => e.ProfilePicture)
                    .WithOne(p => p.User)
                    .HasForeignKey<ProfilePicture>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuration pour l'entité Score
            modelBuilder.Entity<Score>(entity =>
            {
                entity.ToTable("scores");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.TotalScore).HasColumnName("score").IsRequired();
                entity.Property(e => e.LastUpdated).HasColumnName("last_updated")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuration pour l'entité GameState
            modelBuilder.Entity<GameState>(entity =>
            {
                entity.ToTable("gamestates");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.WordId).HasColumnName("word_id");
                entity.Property(e => e.Difficulty).HasColumnName("difficulty");
                entity.Property(e => e.ComboMultiplier).HasColumnName("combo");
            });

            // Configuration pour l'entité ProfilePicture
            modelBuilder.Entity<ProfilePicture>(entity =>
            {
                entity.ToTable("profilepictures");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.Picture).HasColumnName("picture").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("createdat")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"); // Ajusté pour être en UTC

                entity.HasOne(pp => pp.User)
                    .WithOne(u => u.ProfilePicture)
                    .HasForeignKey<ProfilePicture>(pp => pp.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}