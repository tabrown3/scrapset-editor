using System;
using UnityEngine;

namespace Scrapset.Editor
{
    public class SelectionDetection : MonoBehaviour
    {
        public event Action<GameObject> OnGateTouched;
        public event Action<GameObject> OnGateUntouched;

        public bool IsValidSelectionTarget(GameObject gameObject)
        {
            return gameObject.tag == "GateRef";
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsValidSelectionTarget(collision.gameObject))
            {
                OnGateTouched?.Invoke(collision.gameObject);
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (IsValidSelectionTarget(collision.gameObject))
            {
                OnGateUntouched?.Invoke(collision.gameObject);
            }
        }
    }
}
