using System.Collections.Generic;
using UnityEngine;

namespace Extensions.TouchHelperSubClasses
{
    public interface ILocker
	{
		IEnumerable<GameObject> UnlockedObjects { get; }
	}
}