using Scrapset.Engine;
using UnityEngine;

namespace Scrapset.Editor
{
    public class GateRef : MonoBehaviour
    {
        public string SubroutineName { get; set; }
        public int GateId { get; set; }

        public IGate Gate { get; set; }

        SubroutineManager subroutineManager;

        void Start()
        {
            if (subroutineManager == null)
            {
                // use the SubroutineName and GateId to find the actual Gate object
                subroutineManager = FindObjectOfType<SubroutineManager>();
                var srDefinition = subroutineManager.GetDefinition(SubroutineName);
                Gate = srDefinition.GetGateById(GateId);
            }
        }
    }
}
