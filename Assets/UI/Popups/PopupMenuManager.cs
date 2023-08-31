using UnityEngine;
using UnityEngine.UIElements;

namespace Scrapset.UI
{
    public enum Anchor
    {
        Right,
        Left
    }

    public class PopupMenuManager : MonoBehaviour
    {
        [SerializeField] UIDocument scrapsetEditorUI;
        [SerializeField] Camera cam;

        VisualElement layout;
        VisualElement menuContent;

        void Start()
        {
            // LAYOUT
            layout = Resources.Load<VisualTreeAsset>("Popups/PopupMenuLayout")
                .Instantiate()
                .Q("PopupMenu__Container"); // clones asset and attaches to root

            // STYLES
            var styles = Resources.Load<StyleSheet>("Popups/PopupMenuStyles");
            layout.styleSheets.Add(styles);
        }

        public void Attach(GameObject gameObject, VisualElement content, Anchor anchor = Anchor.Right)
        {
            if (layout == null || menuContent != null) return;

            var root = scrapsetEditorUI.rootVisualElement;

            var screenPos = cam.WorldToScreenPoint(gameObject.transform.position);

            // (0,0) in screen space is the bottom left corner. But we want to set top, not bottom,
            //  so subtracting screenPos.y from height.
            layout.style.top = root.resolvedStyle.height - screenPos.y;

            if (anchor == Anchor.Right)
            {
                layout.style.left = screenPos.x;
                layout.style.right = StyleKeyword.Auto;
            } else if (anchor == Anchor.Left)
            {
                layout.style.right = root.resolvedStyle.width - screenPos.x;
                layout.style.left = StyleKeyword.Auto;
            }

            menuContent = content;
            layout.Add(menuContent);

            root.Add(layout);
        }

        public void Detach()
        {
            if (layout == null) return;

            if (menuContent != null)
            {
                layout.Remove(menuContent);
                menuContent = null;
            }

            var root = scrapsetEditorUI.rootVisualElement;

            root.Remove(layout);
        }
    }
}
