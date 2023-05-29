using System.Collections.Generic;
using UnityEngine;

public class EditorManager : MonoBehaviour
{
    [SerializeField] GameObject EntrypointPrefab;
    [SerializeField] GameObject ExpressionPrefab;
    [SerializeField] GameObject SRInputPrefab;
    [SerializeField] GameObject SROutputPrefab;
    [SerializeField] GameObject StatementPrefab;
    [SerializeField] GameObject SubroutinePrefab;
    [SerializeField] GameObject VariablePrefab;

    Dictionary<string, GameObject> srGameObjects = new Dictionary<string, GameObject>();

    SubroutineManager subroutineManager;

    void Start()
    {
        if (subroutineManager == null)
        {
            subroutineManager = FindObjectOfType<SubroutineManager>();
        }
    }
}
