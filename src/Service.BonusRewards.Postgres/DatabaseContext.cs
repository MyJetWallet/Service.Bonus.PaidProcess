using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyJetWallet.Sdk.Postgres;
using Service.BonusRewards.Domain.Models;

namespace Service.BonusRewards.Postgres
{
    public class DatabaseContext : MyDbContext
    {
        public const string Schema = "bonusrewards";

        private const string RewardsTableName = "rewards";
        public DbSet<RewardEntity> RewardEntities { get; set; }

        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            modelBuilder.Entity<RewardEntity>().ToTable(RewardsTableName);
            modelBuilder.Entity<RewardEntity>().HasKey(e => new { e.ClientId, e.RewardId});
            
            modelBuilder.Entity<RewardEntity>().HasIndex(e => e.ClientId);
            modelBuilder.Entity<RewardEntity>().HasIndex(e => e.RewardId);
            modelBuilder.Entity<RewardEntity>().HasIndex(e => e.CampaignId);
                
            modelBuilder.Entity<RewardEntity>().Property(e => e.ClientId).HasMaxLength(128);
            modelBuilder.Entity<RewardEntity>().Property(e => e.ReferrerClientId).HasMaxLength(128);
            modelBuilder.Entity<RewardEntity>().Property(e => e.Asset).HasMaxLength(128);

            base.OnModelCreating(modelBuilder);
        }

        public async Task<int> UpsertAsync(IEnumerable<RewardEntity> entities)
        {
            var result = await RewardEntities.UpsertRange(entities).AllowIdentityMatch().RunAsync();
            return result;
        }
    }
}
