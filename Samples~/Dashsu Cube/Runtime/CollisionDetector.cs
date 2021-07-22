using System;
using UnityEngine;

namespace BennyKok.RuntimeDebug.Demo.DashsuCube
{
    public class CollisionDetector : MonoBehaviour
    {
        public Action<Collision> OnCollisionEnterEvent;
        public Action<Collision> OnCollisionExitEvent;

        private void OnCollisionEnter(Collision other) => OnCollisionEnterEvent?.Invoke(other);
        private void OnCollisionExit(Collision other) => OnCollisionExitEvent?.Invoke(other);
    }
}