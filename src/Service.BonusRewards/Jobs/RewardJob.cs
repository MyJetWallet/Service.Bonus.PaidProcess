using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using MyJetWallet.Sdk.ServiceBus;
using Service.BonusCampaign.Domain.Models.Enums;
using Service.BonusCampaign.Domain.Models.Rewards;
using Service.BonusRewards.Domain.Models;
using Service.BonusRewards.Postgres;
using Service.ChangeBalanceGateway.Grpc;
using Service.ChangeBalanceGateway.Grpc.Models;
using Service.ClientProfile.Grpc;
using Service.ClientProfile.Grpc.Models.Requests;
using Service.ClientWallets.Grpc;
using Service.ClientWallets.Grpc.Models;
using Service.FeeShareEngine.Grpc;
using Service.FeeShareEngine.Grpc.Models;
using AddReferralRequest = Service.FeeShareEngine.Grpc.Models.AddReferralRequest;

namespace Service.BonusRewards.Jobs
{
    public class RewardJob
    {
        private readonly IClientProfileService _clientProfileService;
        private readonly IFeeShareEngineManager _feeShareEngine;
        private readonly ISpotChangeBalanceService _changeBalanceService;
        private readonly IClientWalletService _clientWalletService;
        private readonly IServiceBusPublisher<RewardPaymentMessage> _publisher;
        private readonly ILogger<RewardJob> _logger;
        private readonly DbContextOptionsBuilder<DatabaseContext> _dbContextOptionsBuilder;

        public RewardJob(IClientWalletService clientWalletService, ISpotChangeBalanceService changeBalanceService,
            IFeeShareEngineManager feeShareEngine, IClientProfileService clientProfileService,
            ISubscriber<ExecuteRewardMessage> subscriber, ILogger<RewardJob> logger, IServiceBusPublisher<RewardPaymentMessage> publisher, DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder)
        {
            _clientWalletService = clientWalletService;
            _changeBalanceService = changeBalanceService;
            _feeShareEngine = feeShareEngine;
            _clientProfileService = clientProfileService;
            _logger = logger;
            _publisher = publisher;
            _dbContextOptionsBuilder = dbContextOptionsBuilder;

            subscriber.Subscribe(HandleRewards);
        }

        private async ValueTask HandleRewards(ExecuteRewardMessage message)
        {
            try
            {
                if (Enum.TryParse(message.RewardType, out RewardType type))
                {
                    switch (type)
                    {
                        case RewardType.FeeShareAssignment:
                            await HandleFeeShareAssignment(message);
                            break;
                        case RewardType.ReferrerPaymentAbsolute:
                            await HandlePayments(message, true);
                            break;
                        case RewardType.ClientPaymentAbsolute:
                            await HandlePayments(message, false);
                            break;
                        case RewardType.ReferrerPaymentRelative:
                            break;
                        case RewardType.ClientPaymentRelative:
                            break;
                        default:
                            _logger.LogError(
                                "Unable handle reward message with type {type}, clientId {clientId}, rewardId {rewardId}, campaignId {campaignId}",
                                message.RewardType, message.ClientId, message.RewardId, message.CampaignId);
                            break;
                    }
                }
                else
                {
                    _logger.LogError("Unable to parse reward type {type}", message.RewardType);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable handle reward message with type {type}, clientId {clientId}, rewardId {rewardId}, campaignId {campaignId}", message.RewardType, message.ClientId, message.RewardId, message.CampaignId);
                throw;
            }
        }

        private async Task HandleFeeShareAssignment(ExecuteRewardMessage message)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            Enum.TryParse(message.RewardType, out RewardType type);
            var profile = await _clientProfileService.GetOrCreateProfile(new GetClientProfileRequest
            {
                ClientId = message.ClientId
            });

            if (string.IsNullOrWhiteSpace(profile.ReferrerClientId))
            {
                _logger.LogError("Unable to find referrerId for clientId {clientId}. Reward {rewardId} failed",
                    message.ClientId, message.RewardId);
                await context.UpsertAsync(new[]
                {
                    new RewardEntity
                    {
                        ClientId = message.ClientId,
                        RewardId = message.RewardId,
                        CampaignId = message.CampaignId,
                        RewardType = type.ToString(),
                        Status = RewardStatus.Failed,
                        TimeStamp = DateTime.UtcNow
                    }
                });
                return;
            }

            var feeShareGroupsResponse = await _feeShareEngine.GetAllFeeShareGroups(new PaginationRequest
                { SearchText = message.FeeShareGroup, Take = 1 });
            var feeShareGroup = feeShareGroupsResponse.Groups.FirstOrDefault();

            if (feeShareGroup == null)
            {
                _logger.LogError("Unable to find FeeShareGroup {feeShareGroupId}. Reward {rewardId} failed",message.FeeShareGroup, message.RewardId);
                await context.UpsertAsync(new[]
                {
                    new RewardEntity
                    {
                        ClientId = message.ClientId,
                        RewardId = message.RewardId,
                        CampaignId = message.CampaignId,
                        RewardType = type.ToString(),
                        Status = RewardStatus.Failed,
                        TimeStamp = DateTime.UtcNow
                    }
                });
                return;
            }
            
            var response = await _feeShareEngine.AddReferralLink(new AddReferralRequest
            {
                ClientId = message.ClientId,
                ReferrerClientId = profile.ReferrerClientId,
                FeeShareGroupId = feeShareGroup.GroupId
            });

            if (response.IsSuccess)
                _logger.LogInformation("Reward {rewardId} for client {clientId} executed successfully",
                    message.RewardId, message.ClientId);
            else
            {
                _logger.LogError("Unable to assign client {clientId} to fee share group {feeShareGroupId}. ErrorCode {errorMessage}. Reward {rewardId} failed", message.ClientId, message.FeeShareGroup, response.ErrorCode, message.RewardId);
                Thread.Sleep(10000);
                throw new Exception($"Unable to assign client {message.ClientId} to fee share group {message.FeeShareGroup}. ErrorCode {response.ErrorCode}. Reward {message.RewardId} failed");
            }
            
            await context.UpsertAsync(new[]
            {
                new RewardEntity
                {
                    ClientId = message.ClientId,
                    RewardId = message.RewardId,
                    CampaignId = message.CampaignId,
                    RewardType = type.ToString(),
                    Status = response.IsSuccess ? RewardStatus.Done : RewardStatus.Failed,
                    FeeShareGroup = message.FeeShareGroup,
                    ReferrerClientId = profile.ReferrerClientId,
                    TimeStamp = DateTime.UtcNow
                }
            });
        }

        private async Task HandlePayments(ExecuteRewardMessage message, bool toReferrer)
        {
            Console.WriteLine(JsonSerializer.Serialize(message));
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            Enum.TryParse(message.RewardType, out RewardType type);
            

            var walletsResponse = await _clientWalletService.GetWalletsByClient(new JetClientIdentity()
            {
                ClientId = message.ClientId,
                BrokerId = Program.Settings.DefaultBroker,
                BrandId = Program.Settings.DefaultBrand
            });

            var transactionId = $"{message.ClientId}+|+{message.RewardId}";

            var response = await _changeBalanceService.PayBonusRewardAsync(new FeeTransferRequest
            {
                TransactionId = transactionId,
                ClientId = Program.Settings.BonusServiceClientId,
                FromWalletId = Program.Settings.BonusServiceWalletId,
                ToWalletId = walletsResponse.Wallets.First().WalletId,
                Amount = (double)message.AmountAbs,
                AssetSymbol = message.Asset,
                Comment = $"Reward payment for campaign {message.CampaignId}",
                BrokerId = Program.Settings.DefaultBroker,
                RequestSource = "Service.BonusRewards"
            });

            if (response.Result)
            {
                await _publisher.PublishAsync(new RewardPaymentMessage
                {
                    OperationId = transactionId,
                    ClientId = message.ClientId,
                    WalletId = walletsResponse.Wallets.First().WalletId,
                    Asset = message.Asset,
                    Amount = message.AmountAbs,
                    TimeStamp = DateTime.UtcNow
                });
            }
            
            
            if (response.Result)
                _logger.LogInformation("Reward {rewardId} for client {clientId} executed successfully",
                    message.RewardId, message.ClientId);
            else
            {
                _logger.LogError("Unable to transfer reward to {clientId}. ME response: {errorMessage}. Reward {rewardId} failed", message.ClientId, response.ErrorMessage, message.RewardId);
                Thread.Sleep(10000);
                throw new Exception($"Unable to transfer reward to {message.ClientId}. ME response: {response.ErrorMessage}. Reward {message.RewardId} failed");
            }

            await context.UpsertAsync(new[]
            {
                new RewardEntity
                {
                    ClientId = message.ClientId,
                    RewardId = message.RewardId,
                    CampaignId = message.CampaignId,
                    RewardType = type.ToString(),
                    Status = response.Result ? RewardStatus.Done : RewardStatus.Failed,
                    Asset = message.Asset,
                    AmountAbs = message.AmountAbs,
                    TimeStamp = DateTime.UtcNow
                }
            });
        }
    }
}