using UnityEngine;

namespace Wokarol.GameSystemsLocator
{
    public class SystemOverrider : MonoBehaviour
    {
        private void OnEnable()
        {
            GameSystems.ApplyOverride(gameObject);
        }

        private void OnDisable()
        {
            GameSystems.RemoveOverride(gameObject);
        }
    }
}
