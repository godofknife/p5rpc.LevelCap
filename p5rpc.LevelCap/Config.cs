using p5rpc.LevelCap.Template.Configuration;
using System.ComponentModel;

namespace p5rpc.LevelCap.Configuration
{
    public class Config : Configurable<Config>
    {
        /*
            User Properties:
                - Please put all of your configurable properties here.

            By default, configuration saves as "Config.json" in mod user config folder.    
            Need more config files/classes? See Configuration.cs

            Available Attributes:
            - Category
            - DisplayName
            - Description
            - DefaultValue

            // Technically Supported but not Useful
            - Browsable
            - Localizable

            The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
        */
        [DisplayName("Level Cap Kamoshida Palace")]
        [Description("Set the level cap for Kamoshida Palace")]
        [DefaultValue(15)]
        public int setthelevelcapKamoshida { get; set; } = 15;

        [DisplayName("Level Cap Madarame Palace")]
        [Description("Set the level cap for Madarame Palace")]
        [DefaultValue(25)]
        public int setthelevelcapMadarame { get; set; } = 25;

        [DisplayName("Level Cap Kaneshiro Palace")]
        [Description("Set the level cap for Kaneshiro Palace")]
        [DefaultValue(35)]
        public int setthelevelcapKaneshiro { get; set; } = 35;

        [DisplayName("Level Cap Futaba Palace")]
        [Description("Set the level cap for Futaba Palace")]
        [DefaultValue(45)]
        public int setthelevelcapFutaba { get; set; } = 45;

        [DisplayName("Level Cap Okumura Palace")]
        [Description("Set the level cap for Okumura Palace")]
        [DefaultValue(55)]
        public int setthelevelcapOkumura { get; set; } = 55;

        [DisplayName("Level Cap Sae Palace")]
        [Description("Set the level cap for Sae Palace")]
        [DefaultValue(65)]
        public int setthelevelcapSae { get; set; } = 65;

        [DisplayName("Level Cap Shido Palace")]
        [Description("Set the level cap for Shido Palace")]
        [DefaultValue(75)]
        public int setthelevelcapShido { get; set; } = 75;

        [DisplayName("Level Cap Depth Mementos Palace")]
        [Description("Set the level cap for Depth Mementos Palace")]
        [DefaultValue(85)]
        public int setthelevelcapDepth { get; set; } = 85;

        [DisplayName("Debug Mode")]
        [Description("Logs additional information to the console that is useful for debugging.")]
        [DefaultValue(false)]
        public bool DebugEnabled { get; set; } = false;

        public enum SampleEnum
        {
            NoOpinion,
            Sucks,
            IsMediocre,
            IsOk,
            IsCool,
            ILoveIt
        }
    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}