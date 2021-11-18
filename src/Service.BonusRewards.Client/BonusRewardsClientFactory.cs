using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.BonusRewards.Grpc;

namespace Service.BonusRewards.Client
{
    [UsedImplicitly]
    public class BonusRewardsClientFactory: MyGrpcClientFactory
    {
        public BonusRewardsClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IRewardsService GetRewardsService() => CreateGrpcService<IRewardsService>();
    }
}
