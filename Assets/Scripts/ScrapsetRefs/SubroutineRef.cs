using UnityEngine;

public class SubroutineRef : MonoBehaviour
{
    public string SubroutineName { get; set; }

    static SubroutineManager subroutineManager;

    void Start()
    {
        if (subroutineManager == null)
        {
            subroutineManager = FindObjectOfType<SubroutineManager>();
        }
    }
}
