using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class GateIOLinkTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void GateIOLinkTestsSimplePasses()
    {
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator GateIOLinkTestsWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
