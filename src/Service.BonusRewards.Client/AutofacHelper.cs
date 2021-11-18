using Autofac;
using Service.BonusRewards.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.BonusRewards.Client
{
    public static class AutofacHelper
    {
        public static void RegisterBonusRewardsClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new BonusRewardsClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetRewardsService()).As<IRewardsService>().SingleInstance();
        }
    }
}
