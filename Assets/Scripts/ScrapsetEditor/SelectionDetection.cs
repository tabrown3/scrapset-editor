using System;
using UnityEngine;

namespace Scrapset.Editor
{
    public class SelectionDetection : MonoBehaviour
    {
        public event Action<GameObject> OnGateTouched;
        public event Action<GameObject> OnGateUntouched;

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "GateRef")
            {
                OnGateTouched?.Invoke(collision.gameObject);
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "GateRef")
            {
                OnGateUntouched?.Invoke(collision.gameObject);
            }
        }
    }
}
