using STRINGS;

namespace MegaBattery
{
    class MegaBatteryStrings
    {
        public class BUILDINGS
        {
            public class PREFABS
            {
                public class BATTERYLARGE
                {
                    public static LocString NAME = UI.FormatAsLink("Mega Battery", MegaBatteryConfig.ID);
                    public static LocString DESC = "The battery is large enough to power a rocket.";
                    public static LocString EFFECT = "Stores " + UI.FormatAsLink("Power", "POWER") + " from generators, then provides that power to buildings.\n\nSlightly loses charge over time.\nIt can be stacked vertically.";
                }
            }
        }
    }
}
