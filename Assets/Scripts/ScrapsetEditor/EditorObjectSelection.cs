using System.Collections.Generic;

namespace Scrapset.Editor
{
    public class EditorObjectSelection
    {
        public bool HasSelection
        {
            get
            {
                return gateRefs.Count > 0;
            }
        }

        List<GateRef> gateRefs = new List<GateRef>();
        AABB aabb;

        public void Select(GateRef gateRef)
        {
            if (gateRefs.Contains(gateRef)) { return; }

            gateRefs.Add(gateRef);
        }

        public void Deselect(GateRef gateRef)
        {
            gateRefs.Remove(gateRef);
        }

        public void Clear()
        {
            gateRefs.Clear();
        }
    }
}
