using Scrapset.Engine;
using UnityEngine;

namespace Scrapset.Editor
{
    public class SubroutineRef : MonoBehaviour
    {
        public string SubroutineName { get; set; }

        SubroutineManager subroutineManager;

        void Start()
        {
            if (subroutineManager == null)
            {
                subroutineManager = FindObjectOfType<SubroutineManager>();
            }
        }
    }
}
