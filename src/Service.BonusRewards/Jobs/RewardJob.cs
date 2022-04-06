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
using Service.IndexPrices.Client;
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
        private readonly IIndexPricesClient _pricesClient;

        public RewardJob(IClientWalletService clientWalletService, ISpotChangeBalanceService changeBalanceService,
            IFeeShareEngineManager feeShareEngine, IClientProfileService clientProfileService,
            ISubscriber<ExecuteRewardMessage> subscriber, ILogger<RewardJob> logger, IServiceBusPublisher<RewardPaymentMessage> publisher, DbContextOptionsBuilder<DatabaseContext> dbContextOptionsBuilder, IIndexPricesClient pricesClient)
        {
            _clientWalletService = clientWalletService;
            _changeBalanceService = changeBalanceService;
            _feeShareEngine = feeShareEngine;
            _clientProfileService = clientProfileService;
            _logger = logger;
            _publisher = publisher;
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
            _pricesClient = pricesClient;

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
                        case RewardType.ClientPaymentAbsolute:
                            await HandlePayments(message);
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
                _logger.LogError(e, "Unable to handle reward message with type {type}, clientId {clientId}, rewardId {rewardId}, campaignId {campaignId}", message.RewardType, message.ClientId, message.RewardId, message.CampaignId);
                throw;
            }
        }

        private async Task HandleFeeShareAssignment(ExecuteRewardMessage message)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);
            Enum.TryParse(message.RewardType, out RewardType type);

            var entity = new RewardEntity
            {
                ClientId = message.ClientId,
                RewardId = message.RewardId,
                CampaignId = message.CampaignId,
                RewardType = type.ToString(),
                Status = RewardStatus.New,
                FeeShareGroup = message.FeeShareGroup,
                ReferrerClientId = message.ReferrerClientId,
                ReferralClientId = message.ReferralClientId,
                TimeStamp = DateTime.UtcNow
            };
            await context.UpsertAsync(new[] {entity});
            
            var profile = await _clientProfileService.GetOrCreateProfile(new GetClientProfileRequest
            {
                ClientId = message.ClientId
            });

            if (string.IsNullOrWhiteSpace(profile.ReferrerClientId))
            {
                _logger.LogError("Unable to find referrerId for clientId {clientId}. Reward {rewardId} failed",
                    message.ClientId, message.RewardId);
                
                entity.Status = RewardStatus.Failed;
                await context.UpsertAsync(new[] {entity});

                return;
            }

            var feeShareGroupsResponse = await _feeShareEngine.GetAllFeeShareGroups(new PaginationRequest
                { SearchText = message.FeeShareGroup, Take = 1 });
            var feeShareGroup = feeShareGroupsResponse.Groups.FirstOrDefault();

            if (feeShareGroup == null)
            {
                _logger.LogError("Unable to find FeeShareGroup {feeShareGroupId}. Reward {rewardId} failed",message.FeeShareGroup, message.RewardId);
                entity.Status = RewardStatus.Failed;
                await context.UpsertAsync(new[] {entity});
                return;
            }
            
            var response = await _feeShareEngine.AddReferralLink(new AddReferralRequest
            {
                ClientId = message.ClientId,
                ReferrerClientId = profile.ReferrerClientId,
                FeeShareGroupId = feeShareGroup.GroupId
            });
            
            entity.Status = response.IsSuccess ? RewardStatus.Done : RewardStatus.Failed;
            entity.FeeShareGroup = message.FeeShareGroup;
            entity.ReferrerClientId = profile.ReferrerClientId;
            
            await context.UpsertAsync(new[] {entity});
            
            if (response.IsSuccess)
                _logger.LogInformation("Reward {rewardId} for client {clientId} executed successfully",
                    message.RewardId, message.ClientId);
            else
            {
                _logger.LogError("Unable to assign client {clientId} to fee share group {feeShareGroupId}. ErrorCode {errorMessage}. Reward {rewardId} failed", message.ClientId, message.FeeShareGroup, response.ErrorCode, message.RewardId);
                Thread.Sleep(60000);
                throw new Exception($"Unable to assign client {message.ClientId} to fee share group {message.FeeShareGroup}. ErrorCode {response.ErrorCode}. Reward {message.RewardId} failed");
            }
        }

        private async Task HandlePayments(ExecuteRewardMessage message)
        {
            await using var context = new DatabaseContext(_dbContextOptionsBuilder.Options);

            var existingReward = await context.RewardEntities.FirstOrDefaultAsync(t =>
                t.ClientId == message.ClientId && t.RewardId == message.RewardId);

            if (existingReward != null && existingReward.Status == RewardStatus.Done)
            {
                _logger.LogInformation("Reward {rewardId} for client {clientId} was already executed",
                    message.RewardId, message.ClientId);
                return;
            }

            Enum.TryParse(message.RewardType, out RewardType type);

            if (string.IsNullOrWhiteSpace(message.ClientId))
            {
                _logger.LogError("Unable to process payment without client. Reward {rewardId} failed",  message.RewardId);
                return;
            }

            var entity = new RewardEntity
            {
                ClientId = message.ClientId,
                RewardId = message.RewardId,
                CampaignId = message.CampaignId,
                RewardType = type.ToString(),
                Status = RewardStatus.New,
                Asset = message.Asset,
                AmountAbs = message.AmountAbs,
                TimeStamp = DateTime.UtcNow,
                ClientWalletId = String.Empty,
                IndexPrice = _pricesClient.GetIndexPriceByAssetAsync(message.Asset).UsdPrice,
                ReferralClientId = message.ReferralClientId,
                ReferrerClientId = message.ReferrerClientId
            };
            
            await context.UpsertAsync(new[] {entity});

            var walletsResponse = await _clientWalletService.GetWalletsByClient(new JetClientIdentity()
            {
                ClientId = message.ClientId,
                BrokerId = Program.Settings.DefaultBroker,
                BrandId = Program.Settings.DefaultBrand
            });

            var walletId = walletsResponse.Wallets.First().WalletId;
            var transactionId = $"{message.ClientId}+|+{message.RewardId}";

            var response = await _changeBalanceService.PayBonusRewardAsync(new FeeTransferRequest
            {
                TransactionId = transactionId,
                ClientId = Program.Settings.BonusServiceClientId,
                FromWalletId = Program.Settings.BonusServiceWalletId,
                ToWalletId = walletId,
                Amount = message.AmountAbs,
                AssetSymbol = message.Asset.ToUpper(),
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

            entity.Status = response.Result ? RewardStatus.Done : RewardStatus.Failed;
            entity.ClientWalletId = walletId;
            entity.IndexPrice = _pricesClient.GetIndexPriceByAssetAsync(message.Asset).UsdPrice;
            await context.UpsertAsync(new[] {entity});

            if (response.Result)
                _logger.LogInformation("Reward {rewardId} for client {clientId} executed successfully",
                    message.RewardId, message.ClientId);
            else
            {
                _logger.LogError("Unable to transfer reward to {clientId}. ME response: {errorMessage}. Reward {rewardId} failed", message.ClientId, response.ErrorMessage, message.RewardId);
                Thread.Sleep(60000);
                throw new Exception($"Unable to transfer reward to {message.ClientId}. ME response: {response.ErrorMessage}. Reward {message.RewardId} failed");
            }
        }
    }
}