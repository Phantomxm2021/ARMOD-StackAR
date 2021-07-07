using System.Collections;
using System.Collections.Generic;
using com.Phantoms.ActionNotification.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class actionnotificationmanager_Tests_NewTestScript
    {
        // A Test behaves as an ordinary method
        [Test]
        public void actionnotificationmanager_Tests_NewTestScriptSimplePasses()
        {
            // Use the Assert class to test conditions
            ActionNotificationCenter.DefaultCenter.AddObserver(TestObserver1, "TestObserver");
            ActionNotificationCenter.DefaultCenter.AddObserver(TestObserver2, "TestObserver");
            ActionNotificationCenter.DefaultCenter.PostNotification("TestObserver", new BaseNotificationData());
            ActionNotificationCenter.DefaultCenter.RemoveObserver("TestObserver", TestObserver1);
            ActionNotificationCenter.DefaultCenter.RemoveObserver("TestObserver", TestObserver2);
            ActionNotificationCenter.DefaultCenter.PostNotification("TestObserver", new BaseNotificationData());

//            ActionNotificationCenter.DefaultCenter.RemoveObserver("TestObserver");
//            ActionNotificationCenter.DefaultCenter.PostNotification("TestObserver", new BaseNotificationData());
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator actionnotificationmanager_Tests_NewTestScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }




        [Test]
        public void Test_ResultAction()
        {
            ActionNotificationCenter.DefaultCenter.AddObserver(TestResult,"Test");
            
            ActionNotificationCenter.DefaultCenter.RemoveObserver("Test");
            var tmp_Results = ActionNotificationCenter.DefaultCenter.PostNotificationWithResult("Test",new BaseNotificationData());
            Assert.AreEqual(tmp_Results,null);
        
        }


        private object TestResult(BaseNotificationData _base)
        {
            return "TestResult";
        }
        
        private void TestObserver1(BaseNotificationData _notificationData)
        {
            Debug.Log("Hi This is TestObserver1");
        }

        private void TestObserver2(BaseNotificationData _notificationData)
        {
            Debug.Log("Hi This is TestObserver2");
        }
    }
}