using STRINGS;

namespace DoubleBed
{
	class STRINGS
	{
		public class BUILDINGS
		{
			public class PREFABS
			{
				public class DOUBLEBED
				{
					public static LocString NAME = UI.FormatAsLink("Double Bed", "DOUBLEBED");
					public static LocString DESC = "The more the merrier.";
					public static LocString EFFECT = "Giwes two Duplicants a place to sleep.\n\nAt night, Duplicants will occupy an empty bed, and in the morning they will free it for other Duplicants.";
				}
			}
		}

		public static void DoReplacement()
		{
			LocString.CreateLocStringKeys(typeof(STRINGS), "");
		}
	}
}
