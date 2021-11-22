using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.BonusRewards.Domain.Models;

namespace Service.BonusRewards.Grpc.Models
{
    [DataContract]
    public class RewardsResponse 
    {
        [DataMember(Order = 1)] public List<RewardEntity> Rewards { get; set; }
    }
}