using System;
using Microsoft.EntityFrameworkCore;
using TableTennis.DataAccess.DBAccess.Models;

namespace TableTennis.DataAccess.DBAccess
{
    public class PostgreSqlDbContext : DbContext
    {
        public DbSet<GamesScoresMap> GamesScoresMap { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<PlayerInfo> PlayerInfos { get; set; }
        public DbSet<Score> Scores { get; set; }

        public PostgreSqlDbContext(DbContextOptions<PostgreSqlDbContext> options)
            : base(options)
        {
        }

        public PostgreSqlDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GamesScoresMap>()
                .HasKey(gs => new {gs.GameId, gs.ScoreId});

            modelBuilder.Entity<GamesScoresMap>()
                .HasOne(gs => gs.Game)
                .WithMany(g => g.Scores)
                .HasForeignKey(gs => gs.GameId);

            modelBuilder.Entity<GamesScoresMap>()
                .HasOne(gs => gs.Score)
                .WithMany(g => g.Games)
                .HasForeignKey(gs => gs.ScoreId);
        }
    }
}