using System;
using UnityEngine;

namespace Wokarol.GameSystemsLocator.Components
{
    [DefaultExecutionOrder(-500)]
    public class SystemLocatorRoot : MonoBehaviour
    {
        public event Action Destroyed;

        private void OnDestroy()
        {
            Destroyed?.Invoke();
        }
    }
}