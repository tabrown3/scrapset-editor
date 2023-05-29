using UnityEngine;

public class GateRef : MonoBehaviour
{
    public string SubroutineName { get; set; }
    public int GateId { get; set; }

    SubroutineManager subroutineManager;

    void Start()
    {
        if (subroutineManager == null)
        {
            subroutineManager = FindObjectOfType<SubroutineManager>();
        }
    }
}