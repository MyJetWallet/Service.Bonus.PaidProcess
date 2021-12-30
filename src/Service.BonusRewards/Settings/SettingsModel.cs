using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.BonusRewards.Settings
{
    public class SettingsModel
    {
        [YamlProperty("BonusRewards.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("BonusRewards.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("BonusRewards.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("BonusRewards.BonusServiceClientId")]
        public string BonusServiceClientId { get; set; }
        
        [YamlProperty("BonusRewards.BonusServiceWalletId")]
        public string BonusServiceWalletId { get; set; }
        
        [YamlProperty("BonusRewards.DefaultBroker")]
        public string DefaultBroker { get; set; }
        
        [YamlProperty("BonusRewards.DefaultBrand")]
        public string DefaultBrand { get; set; }

        [YamlProperty("BonusRewards.PostgresConnectionString")]
        public string PostgresConnectionString { get; set; }
        
        [YamlProperty("BonusRewards.ChangeBalanceGatewayGrpcServiceUrl")]
        public string ChangeBalanceGatewayGrpcServiceUrl { get; set; }
        
        [YamlProperty("BonusRewards.ClientProfileGrpcServiceUrl")]
        public string ClientProfileGrpcServiceUrl { get; set; }
        
        [YamlProperty("BonusRewards.ClientWalletsGrpcServiceUrl")]
        public string ClientWalletsGrpcServiceUrl { get; set; }
        
        [YamlProperty("BonusRewards.FeeShareGrpcServiceUrl")]
        public string FeeShareGrpcServiceUrl { get; set; }
        
        [YamlProperty("BonusRewards.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }
        
        [YamlProperty("BonusRewards.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }
    }
}
