using System.Collections.Generic;
using UnityEngine;

public class Processor : MonoBehaviour
{
    int idCounter = 0;
    Dictionary<int, IExecutable> gates = new Dictionary<int, IExecutable>();
    Dictionary<int, GameObject> gateObjects = new Dictionary<int, GameObject>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public int SpawnGate<T>(string name) where T : MonoBehaviour, IExecutable, new()
    {
        var tempGameObj = new GameObject(name, typeof(T));
        var gate = tempGameObj.GetComponent<IExecutable>();
        gate.Id = idCounter++;
        Debug.Log(gate.Id);
        gates.Add(gate.Id, gate);
        gateObjects.Add(gate.Id, tempGameObj);
        tempGameObj.transform.parent = transform;

        return gate.Id;
    }
}
