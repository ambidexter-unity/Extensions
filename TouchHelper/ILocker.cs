using System.Collections.Generic;
using UnityEngine;

namespace Common.TouchHelper
{
	public interface ILocker
	{
		IEnumerable<GameObject> UnlockedObjects { get; }
	}
}