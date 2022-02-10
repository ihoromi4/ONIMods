using STRINGS;

namespace Bioreactor
{
	public class STRINGS
	{
		public const string PRODUCTIVITY_KEY = "{PRODUCTIVITY}";

		public class BUILDINGS
		{
			public class PREFABS
			{
				public class BIOREACTOR
				{
					public static LocString NAME = UI.FormatAsLink("Bioreactor", "BIOREACTOR");
					public static LocString DESC = "Used to break organic to components.";
					public static LocString EFFECT = $"Consumes {ELEMENTS.TOXICSAND.NAME} " +
						$"to grow Food Poisoning germs and produce {ELEMENTS.DIRT.NAME}, {ELEMENTS.CONTAMINATEDOXYGEN.NAME} and {ELEMENTS.METHANE.NAME}.";

					public class DESCRIPTORS
					{
						public static LocString TEXT = "Optimal <style=\"KKeyword\">temperature</style>: {OPTIMAL_TEMP}";
						public static LocString TOOLTIP = "Maximum productivity is achieved at optimal temperature";
					}
				}
			}
		}

		public class BUILDING
		{
			public class STATUSITEMS
			{
				public class PRODUCTIVITY_STATUS
				{
					public static LocString NAME = $"Productivity: {STRINGS.PRODUCTIVITY_KEY} %";
					public static LocString TOOLTIP = "Productivity of this building depends of load mass and temperature";
					public static LocString NOTIFICATION_NAME = "";
					public static LocString NOTIFICATION_TOOLTIP = "";
				}
				public class GASS_STORAGE_FULL_STATUS
				{
					public static LocString NAME = "Gas storage is full";
					public static LocString TOOLTIP = "Gas storage can contain limeted amount of gas";
					public static LocString NOTIFICATION_NAME = "";
					public static LocString NOTIFICATION_TOOLTIP = "";
				}
			}
		}
	}
}
