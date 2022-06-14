using UnityEngine;

namespace Wokarol.GameSystemsLocator
{
    public class SystemOverrider : MonoBehaviour
    {
        public void OnEnable()
        {
            GameSystems.ApplyOverride(gameObject);
        }

        private void OnDisable()
        {
            GameSystems.RemoveOverride(gameObject);
        }
    }
}
