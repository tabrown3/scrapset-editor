using System.Collections.Generic;
using UnityEngine;

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

        List<GameObject> gateRefs = new List<GameObject>();
        AABB aabb;

        public void Select(GameObject gameObject)
        {
            if (gateRefs.Contains(gameObject)) { return; }

            gateRefs.Add(gameObject);
        }

        public void Deselect(GameObject gameObject)
        {
            gateRefs.Remove(gameObject);
        }

        public void Clear()
        {
            gateRefs.Clear();
        }
    }
}
