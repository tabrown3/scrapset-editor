using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scrapset.UI
{
    public enum Anchor
    {
        Right,
        Left,
        Top,
        Bottom
    }

    public class PopupMenuManager : MonoBehaviour
    {
        [SerializeField] UIDocument scrapsetEditorUI;
        [SerializeField] Camera cam;
        [SerializeField] UIInputManager uiInputManager;

        VisualElement layout;

        GameObject attachedGameObject;
        VisualElement attachedContent;
        Anchor attachedAnchor;

        void Start()
        {
            // LAYOUT
            layout = Resources.Load<VisualTreeAsset>("Popups/PopupMenuLayout")
                .Instantiate()
                .Q("PopupMenu__Container"); // clones asset and attaches to root

            // STYLES
            var styles = Resources.Load<StyleSheet>("Popups/PopupMenuStyles");
            layout.styleSheets.Add(styles);

            RegisterAutoDetachHandlers();
            uiInputManager.RegisterInteractiveUI(layout);
        }

        public void Attach(GameObject gameObject, VisualElement content, Anchor anchor = Anchor.Right)
        {
            if (layout == null) return;

            if (attachedContent != null)
            {
                Detach();
            }

            // store attachment inputs
            attachedGameObject = gameObject;
            attachedContent = content;
            attachedAnchor = anchor;

            RenderAtAttachedPos();

            layout.Add(attachedContent);

            var root = scrapsetEditorUI.rootVisualElement;
            root.Add(layout);
        }

        public void Detach()
        {
            if (layout == null) return;

            // clear attachment inputs
            attachedGameObject = null;
            attachedAnchor = Anchor.Right;

            if (attachedContent != null)
            {
                layout.Remove(attachedContent);
                attachedContent = null;
            }

            var root = scrapsetEditorUI.rootVisualElement;

            root.Remove(layout);
        }

        void Update()
        {
            RenderAtAttachedPos();
        }

        void RenderAtAttachedPos()
        {
            if (attachedGameObject == null) return;

            var root = scrapsetEditorUI.rootVisualElement;

            var screenPos = cam.WorldToScreenPoint(attachedGameObject.transform.position);

            // (0,0) in screen space is the bottom left corner. But we want to set top, not bottom,
            //  so subtracting screenPos.y from height.
            if (attachedAnchor == Anchor.Right || attachedAnchor == Anchor.Left)
            {
                layout.style.top = root.resolvedStyle.height - screenPos.y;

                if (attachedAnchor == Anchor.Right)
                {
                    layout.style.left = screenPos.x;
                    layout.style.right = StyleKeyword.Auto;
                } else if (attachedAnchor == Anchor.Left)
                {
                    layout.style.right = root.resolvedStyle.width - screenPos.x;
                    layout.style.left = StyleKeyword.Auto;
                }
            } else if (attachedAnchor == Anchor.Top || attachedAnchor == Anchor.Bottom)
            {
                // positioning for Anchor.Right
                layout.style.left = screenPos.x;
                layout.style.right = StyleKeyword.Auto;

                if (attachedAnchor == Anchor.Top)
                {
                    layout.style.bottom = screenPos.y;
                    layout.style.top = StyleKeyword.Auto;
                } else if (attachedAnchor == Anchor.Bottom)
                {
                    // currently the same as Anchor.Right
                    layout.style.top = root.resolvedStyle.height - screenPos.y;
                    layout.style.bottom = StyleKeyword.Auto;
                }
            } else
            {
                throw new Exception($"Could not anchor context menu: anchor position {attachedAnchor} is invalid");
            }
        }

        // The auto-detach handlers are two handlers for the same event, PointerDownEvent,
        //  registered one against the root, the other against the popup. Because the child
        //  handler always executes first (by default), it sets a flag to tell the root handler
        //  NOT to detach if the click originated from the popup.
        // Ultimately this makes it to where clicking anywhere outside the popup closes it, but
        //  clicking anywhere inside the popup does not close it.
        void RegisterAutoDetachHandlers()
        {
            var root = scrapsetEditorUI.rootVisualElement;
            var fromPopup = false;
            root.RegisterCallback<PointerDownEvent>(e =>
            {
                if (!fromPopup && attachedGameObject != null)
                {
                    Detach();
                }
                fromPopup = false;
            });
            layout.RegisterCallback<PointerDownEvent>(e =>
            {
                fromPopup = true;
            });
        }
    }
}
