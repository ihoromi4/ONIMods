using STRINGS;

namespace BatteriesChargeSensor
{
    class STRINGS
	{
		public class BUILDINGS
		{
			public class PREFABS
			{
				public class BATTERIESCHARGESENSOR
				{
					public static LocString NAME = UI.FormatAsLink("Batteries Charge Sensor", "BATTERIESCHARGESENSOR");
					public static LocString DESC = "Charge sensor measures the energy stored in all batteries connected to the power network and sends logical signal.";
					public static LocString EFFECT = "Sends a <b><style=\"logic_on\">Green Signal</style></b> or <b><style=\"logic_off\">Red Signal</style></b> when the <link=\"POWER\">energy</link> stored in connected power network reaches specified value.";
					public static LocString LOGIC_PORT = "Stored <link=\"POWER\">energy</link>";
					public static LocString LOGIC_PORT_ACTIVE = "Sends <b><style=\"logic_on\">Green Signal</style></b> until stored energy level increases to the high threshold";
					public static LocString LOGIC_PORT_INACTIVE = "Sends <b><style=\"logic_off\">Red Signal</style></b> until stored energy level reduces to the low threshold";
				}
			}
		}

		public class BUILDING
		{
			public class STATUSITEMS
			{
				public class STOREDENERGY_STATUS
				{
					public static LocString NAME = "Stored energy: {JoulesAvailable}/{JoulesCapacity}";
					public static LocString TOOLTIP = "Energy stored in all batteries connected to the network";
					public static LocString NOTIFICATION_NAME = "";
					public static LocString NOTIFICATION_TOOLTIP = "";
				}
			}
		}

		public static void DoReplacement()
		{
			LocString.CreateLocStringKeys(typeof(STRINGS), "");
		}
	}
}
