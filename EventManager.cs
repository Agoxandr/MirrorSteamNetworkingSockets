using UnityEngine;

namespace Agoxandr.Utils
{
    public class EventManager : MonoBehaviour
    {
        public delegate void UpdateAction();
        public static event UpdateAction OnUpdated;

        public delegate void FixedUpdateAction();
        public static event FixedUpdateAction OnFixedUpdated;

        public delegate void LateUpdateAction();
        public static event LateUpdateAction OnLateUpdated;

        private void Update()
        {
            OnUpdated?.Invoke();
        }

        private void FixedUpdate()
        {
            OnFixedUpdated?.Invoke();
        }

        private void LateUpdate()
        {
            OnLateUpdated?.Invoke();
        }
    }
}
