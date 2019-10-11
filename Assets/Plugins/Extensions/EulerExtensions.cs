using UnityEngine;

namespace Extensions
{
    public static class EulerExtensions
	{
		public static Vector3 ShortestFrom(this Vector3 to, Quaternion from)
		{
			return to.ShortestFrom(from.eulerAngles);
		}

		public static Vector3 ShortestFrom(this Vector3 to, Vector3 from)
		{
			return new Vector3(CalcShortest(from.x, to.x),
				CalcShortest(from.y, to.y), CalcShortest(from.z, to.z));
		}

		private static float CalcShortest(float from, float to)
		{
			var d = Mathf.Abs(to - from);
			if (d > 180f)
			{
				if (to > from)
				{
					return from - 360f + d;
				}

				return from + 360f - d;
			}

			return to;
		}
	}
}