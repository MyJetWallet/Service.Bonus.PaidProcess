using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.BonusRewards.Domain.Models;
using Service.BonusRewards.Jobs;
using Service.ChangeBalanceGateway.Client;
using Service.ClientProfile.Client;
using Service.ClientWallets.Client;
using Service.FeeShareEngine.Client;

namespace Service.BonusRewards.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var serviceBusClient =
                builder.RegisterMyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort), Program.LogFactory);
            var queueName = "Service.BonusReward";

            builder.RegisterMyServiceBusPublisher<RewardPaymentMessage>(serviceBusClient,
                RewardPaymentMessage.TopicName, false);
            builder.RegisterMyServiceBusSubscriberSingle<ExecuteRewardMessage>(serviceBusClient,
                ExecuteRewardMessage.TopicName, queueName, TopicQueueType.PermanentWithSingleConnection);
            
            builder.RegisterClientProfileClientWithoutCache(Program.Settings.ClientProfileGrpcServiceUrl);
            builder.RegisterClientWalletsClientsWithoutCache(Program.Settings.ClientWalletsGrpcServiceUrl);
            builder.RegisterFeeShareEngineClient(Program.Settings.FeeShareGrpcServiceUrl);
            builder.RegisterSpotChangeBalanceGatewayClient(Program.Settings.ChangeBalanceGatewayGrpcServiceUrl);

            builder.RegisterType<RewardJob>().AsSelf().AutoActivate().SingleInstance();
        }
    }
}