
using Scrapset.Editor;
using Scrapset.Engine;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scrapset.UI
{
    public class ProgramFlowContextMenu : MonoBehaviour, IContextMenu
    {
        PopupMenuManager popupMenuManager;

        void Start()
        {
            popupMenuManager = GetComponent<PopupMenuManager>();
        }

        public void Attach(GameObject gameObject, IGate gate, PortDirection portDirection, Anchor anchor = Anchor.Right)
        {
            var listView = new ListView();
            FillPortList(listView, gate, portDirection);

            popupMenuManager.Attach(gameObject, listView, anchor);
        }

        void FillPortList(ListView listView, IGate gate, PortDirection portDirection)
        {

        }
    }
}
