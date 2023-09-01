using Scrapset.Editor;
using Scrapset.Engine;
using UnityEngine;

namespace Scrapset.UI
{
    public interface IContextMenu
    {
        void Attach(GameObject gameObject, IGate gate, PortDirection portDirection, Anchor anchor);
    }
}
