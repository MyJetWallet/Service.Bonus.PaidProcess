using System.ServiceModel;
using System.Threading.Tasks;
using Service.BonusRewards.Grpc.Models;

namespace Service.BonusRewards.Grpc
{
    [ServiceContract]
    public interface IRewardsService
    {
        [OperationContract]
        Task<RewardsResponse> GetRewards();
        
        [OperationContract]
        WalletResponse GetWalletInfo();
    }
}