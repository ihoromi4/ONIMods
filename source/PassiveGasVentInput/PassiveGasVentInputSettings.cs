using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace PassiveGasVentInput
{
    [JsonObject(MemberSerialization.OptIn)]
    [ModInfo("https://github.com/ihoromi4/ONIMods")]
    [RestartRequired]
    class PassiveGasVentInputSettings : SingletonOptions<PassiveGasVentInputSettings>
    {
        [JsonProperty]
        [Option("Minimum pressure", "Minimum gas pressure to start working.\nDefault = 2 Kg", Format = "F3")]
        public float MinimumPressure { get; set; }

        [JsonProperty]
        [Option("Minimum flow", "Gas flow at minimum pressure.\nDefault = 0.0 Kg", Format = "F3")]
        public float MinimumFlow { get; set; }

        [JsonProperty]
        [Option("Maximum pressure", "Gas pressure which produces maximum flow.\nDefault = 602 Kg", Format = "F3")]
        public float MaximumPressure { get; set; }

        [JsonProperty]
        [Option("Maximum flow", "Maximum achievable gas flow.\nDefault = 3 Kg", Format = "F3")]
        public float MaximumFlow { get; set; }

        public PassiveGasVentInputSettings()
        {
            MinimumPressure = 2.0f;
            MinimumFlow = 0.0f;
            MaximumPressure = 602.0f;
            MaximumFlow = 3.0f;
        }
    }
}
