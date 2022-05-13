using System.Runtime.Serialization;

namespace Service.BonusRewards.Domain.Models
{
    [DataContract]
    public class FailedRewardPaymentMessage
    {
        public const string TopicName = "bonus-failed-reward-payment";

        [DataMember(Order = 1)] public RewardEntity Reward { get; set; }
        [DataMember(Order = 2)] public RewardPaymentErrorType ErrorType { get; set; }
    }
}