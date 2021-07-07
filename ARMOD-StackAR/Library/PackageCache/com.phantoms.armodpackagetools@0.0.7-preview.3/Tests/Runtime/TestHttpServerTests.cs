using System;
using System.Collections;
using System.Collections.Generic;
using com.Phantoms.ARMODPackageTools.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace Tests
{
    public class TestHttpServerTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestHttpServerTestsSimplePasses()
        {
            // Use the Assert class to test conditions


            
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestHttpServerTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        
    }
}