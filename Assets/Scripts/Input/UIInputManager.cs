using UnityEngine;
using UnityEngine.UIElements;

namespace Scrapset
{
    public class UIInputManager : MonoBehaviour
    {
        [SerializeField] UIDocument doc;

        public bool IsPointerOverUI { get; private set; } = false;

        void OnEnable()
        {
            var root = doc.rootVisualElement;
            RegisterEnterExitEvents(root);
        }

        void RegisterEnterExitEvents(VisualElement root)
        {
            var programmingPanel = root.Q<VisualElement>("ProgrammingPanel");
            var contextPanel = root.Q<VisualElement>("ContextPanel");
            var readoutPanel = root.Q<VisualElement>("ReadoutPanel");

            programmingPanel.RegisterCallback<PointerEnterEvent>(HandlePointerEnter);
            contextPanel.RegisterCallback<PointerEnterEvent>(HandlePointerEnter);
            readoutPanel.RegisterCallback<PointerEnterEvent>(HandlePointerEnter);

            programmingPanel.RegisterCallback<PointerLeaveEvent>(HandlePointerLeave);
            contextPanel.RegisterCallback<PointerLeaveEvent>(HandlePointerLeave);
            readoutPanel.RegisterCallback<PointerLeaveEvent>(HandlePointerLeave);
        }

        void HandlePointerEnter(PointerEnterEvent e)
        {
            IsPointerOverUI = true;
        }

        void HandlePointerLeave(PointerLeaveEvent e)
        {
            IsPointerOverUI = false;
        }
    }
}
