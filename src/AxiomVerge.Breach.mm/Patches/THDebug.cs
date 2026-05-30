using Breach.Tools;
using log4net;
using MonoMod;

namespace OuterBeyond
{
	internal class THDebug
	{
		private static THLogPriority mMinOutputLevel;
		private static readonly ILog mLog = Log.Initialize();

		[MonoModConstructor]
		public static void StaticConstructor()
		{
			// Do nothing here, omits the original mLog assignment from overriding ours
		}

		public static void Initialize()
        {
            // Do nothing here, below initializer is called in the patched Program.Main
        }

		internal static void _Initialize()
		{
            mMinOutputLevel = THLogPriority.DEBUG;
		}
    }
}