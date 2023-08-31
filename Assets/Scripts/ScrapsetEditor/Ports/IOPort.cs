using Scrapset.UI;
using UnityEngine;

namespace Scrapset.Editor
{
    public enum PortDirection
    {
        Input,
        Output
    }

    public class IOPort : MonoBehaviour
    {
        [SerializeField] PortDirection portDirection;
        [SerializeField] Anchor anchor;

        GateIOContextMenu menu;
        WorldCursor worldCursor;
        GateRef gateRef;

        private void Start()
        {
            menu = FindObjectOfType<GateIOContextMenu>();
            worldCursor = FindObjectOfType<WorldCursor>();
            gateRef = transform.parent.GetComponent<GateRef>();
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (menu == null || worldCursor == null) return;
            if (collision.gameObject != worldCursor.gameObject) return; // only attach on WorldCursor collision

            menu.Attach(gameObject, gateRef.Gate, portDirection, anchor);
        }
    }
}
