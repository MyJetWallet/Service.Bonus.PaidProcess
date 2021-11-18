using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.BonusRewards.Grpc;
using Service.BonusRewards.Grpc.Models;
using Service.BonusRewards.Postgres;
using Service.BonusRewards.Settings;

namespace Service.BonusRewards.Services
{
    public class RewardsService: IRewardsService
    {
        private readonly ILogger<RewardsService> _logger;
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public RewardsService(ILogger<RewardsService> logger, DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _logger = logger;
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task<RewardsResponse> GetRewards()
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            var rewards = await context.RewardEntities.ToListAsync();
            return (new RewardsResponse
            {
                Rewards = rewards.OrderByDescending(t=>t.TimeStamp).ToList()
            });
        }
    }
}
