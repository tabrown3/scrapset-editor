using Scrapset.UI;
using UnityEngine;

namespace Scrapset.Editor
{
    public class IOPort : MonoBehaviour
    {
        [SerializeField] RenderSide renderSide;

        GateIOContextMenu menu;
        WorldCursor worldCursor;

        private void Start()
        {
            menu = FindObjectOfType<GateIOContextMenu>();
            worldCursor = FindObjectOfType<WorldCursor>();
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (menu == null || worldCursor == null) return;
            if (collision.gameObject != worldCursor.gameObject) return; // only attach on WorldCursor collision

            menu.Attach(gameObject, renderSide);
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (menu == null || worldCursor == null) return;

            menu.Detach();
        }
    }
}
