using System;
using System.Runtime.Serialization;

namespace Service.BonusRewards.Domain.Models
{
    [DataContract]
    public class RewardPaymentMessage
    {
        public const string TopicName = "bonus-reward-payment";
        [DataMember(Order = 1)] public string ClientId { get; set; }
        [DataMember(Order = 2)] public string WalletId { get; set; }
        [DataMember(Order = 3)] public string Asset { get; set; }
        [DataMember(Order = 4)] public decimal Amount { get; set; }
        [DataMember(Order = 5)] public DateTime TimeStamp { get; set; }
    }
}