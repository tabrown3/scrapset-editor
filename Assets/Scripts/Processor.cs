using UnityEngine;

public class Processor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SpawnGate<T>(string name) where T : MonoBehaviour, IExecutable, new()
    {
        var tempGameObj = new GameObject(name, typeof(T));
        tempGameObj.transform.parent = transform;
    }
}
