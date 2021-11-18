using System.Runtime.Serialization;
using Service.BonusCampaign.Domain.Models.Rewards;

namespace Service.BonusRewards.Domain.Models
{
    [DataContract]
    public class ExecuteRewardMessage
    {
        public const string TopicName = "bonus-reward-execution";
        
        [DataMember(Order = 1)] public string ClientId { get; set; }
        [DataMember(Order = 2)] public RewardType RewardType { get; set; }
        [DataMember(Order = 3)] public string FeeShareGroup { get; set; }
        [DataMember(Order = 4)] public string Asset { get; set; }
        [DataMember(Order = 5)] public decimal AmountAbs { get; set; }
        [DataMember(Order = 6)] public string RewardId { get; set; }
        [DataMember(Order = 7)] public string CampaignId { get; set; }
        [DataMember(Order = 8)] public decimal AmountRel { get; set; }
        [DataMember(Order = 9)] public decimal Percentage { get; set; }
    }
}