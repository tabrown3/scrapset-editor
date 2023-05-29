using System.Collections.Generic;
using UnityEngine;

public class ScrapsetEditorController : MonoBehaviour
{
    [SerializeField] SubroutineManager SubroutineManager;

    [SerializeField] GameObject SubroutineDefinitionPrefab;
    [SerializeField] GameObject EntrypointPrefab;
    [SerializeField] GameObject ExpressionPrefab;
    [SerializeField] GameObject SRInputPrefab;
    [SerializeField] GameObject SROutputPrefab;
    [SerializeField] GameObject StatementPrefab;
    [SerializeField] GameObject SubroutinePrefab;
    [SerializeField] GameObject VariablePrefab;

    Dictionary<string, GameObject> srGameObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        var mainName = "Main";
        if (!SubroutineManager.HasDefinition(mainName))
        {
            SubroutineManager.DeclareSubroutine(mainName);
        }

        CreateSubroutineRef(mainName);
    }

    void CreateSubroutineRef(string name)
    {
        var gameObject = Instantiate(SubroutineDefinitionPrefab);
        gameObject.name = name;
        gameObject.transform.SetParent(transform);

        var subroutineRef = gameObject.GetComponent<SubroutineRef>();
        subroutineRef.SubroutineName = name;

        srGameObjects.Add(name, gameObject);
    }
}
