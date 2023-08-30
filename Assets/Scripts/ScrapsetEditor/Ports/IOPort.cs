using Scrapset.UI;
using UnityEngine;

namespace Scrapset.Editor
{
    public class IOPort : MonoBehaviour
    {
        [SerializeField] RenderSide renderSide;

        GateIOContextMenu menu;

        private void Start()
        {
            menu = FindObjectOfType<GateIOContextMenu>();
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (menu == null) return;

            menu.Attach(gameObject, renderSide);
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (menu == null) return;

            menu.Detach();
        }
    }
}
