using System;
using System.Runtime.Serialization;
using Service.BonusCampaign.Domain.Models.Enums;
using Service.BonusCampaign.Domain.Models.Rewards;

namespace Service.BonusRewards.Domain.Models
{
    [DataContract]
    public class RewardEntity
    {
        [DataMember(Order = 1)]public string ClientId { get; set; }
        [DataMember(Order = 2)]public string RewardId { get; set; }
        [DataMember(Order = 3)]public string CampaignId { get; set; }
        [DataMember(Order = 4)] public string RewardType { get; set; }
        [DataMember(Order = 5)]public RewardStatus Status { get; set; }
        
        //FeeShare reward fields
        [DataMember(Order = 6)] public string FeeShareGroup { get; set; }
        [DataMember(Order = 7)]public string ReferrerClientId { get; set; }
        
        //payment reward fields
        [DataMember(Order = 8)]public string Asset { get; set; }
        [DataMember(Order = 9)]public decimal AmountAbs { get; set; }
        [DataMember(Order = 10)]public decimal AmountRel { get; set; }
        [DataMember(Order = 11)]public decimal Percentage { get; set; }    
        [DataMember(Order = 12)] public DateTime TimeStamp { get; set; }
    }
}