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
    SubroutineDefinition activeSRDefinition;

    void Start()
    {
        var mainName = "Main";
        if (!SubroutineManager.HasDefinition(mainName))
        {
            activeSRDefinition = SubroutineManager.DeclareSubroutine(mainName);
            CreateSubroutineRef(mainName);
        }
    }

    private void SetActiveSubroutine(string name)
    {
        if (!SubroutineManager.HasDefinition(name))
        {
            throw new System.Exception($"Unable to set active subroutine: '{name}' does not exist");
        }

        activeSRDefinition = SubroutineManager.GetDefinition(name);
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

    void CreateGateRef(int id)
    {
        var gate = activeSRDefinition.GetGateById(id);

    }
}
