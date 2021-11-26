using System.Runtime.Serialization;

namespace Service.BonusRewards.Grpc.Models
{
    [DataContract]
    public class WalletResponse
    {
        [DataMember(Order = 1)] public string ClientId { get; set; }
        [DataMember(Order = 2)] public string WalletId { get; set; }
        [DataMember(Order = 3)] public string BrokerId { get; set; }
    }
}