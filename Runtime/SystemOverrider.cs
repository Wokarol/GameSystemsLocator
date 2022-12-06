using System.Collections.Generic;
using UnityEngine;

namespace Wokarol.GameSystemsLocator
{
    public class SystemOverrider : MonoBehaviour
    {
        [SerializeField] private bool grabSystemsFromChildren;
        [SerializeField] private List<GameObject> systems;

        private void OnValidate()
        {
            if (Application.isPlaying && isActiveAndEnabled)
            {
                Debug.LogError("The values of this component should not be changed at runtime while the component is active");
            }
        }

        private void OnEnable()
        {
            var holder = grabSystemsFromChildren ? gameObject : null;
            GameSystems.TryApplyOverride(holder, systems);
        }

        private void OnDisable()
        {
            var holder = grabSystemsFromChildren ? gameObject : null;
            GameSystems.RemoveOverride(holder, systems);
        }
    }
}
