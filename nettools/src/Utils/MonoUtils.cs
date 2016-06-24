using System;

namespace nettools
{

    internal static class MonoUtils
	{

		public static bool IsRunningOnMono()
		{
			return Type.GetType("Mono.Runtime") != null;
		}

	}

}
