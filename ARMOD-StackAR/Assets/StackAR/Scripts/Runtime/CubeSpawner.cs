using UnityEngine;
using XRMODEngineCore.Runtime;

namespace StackAR.Runtime
{
    public class CubeSpawner : XRMODBehaviour
    {
        public GameObject cube;
        public Transform root;
        public CubeDirection CubeDirection;
        [SerializeField]private Transform selfTrans;
        private Transform startPoint;
        

        private void Start()
        {
            selfTrans = transform;
            startPoint = GameObject.Find("Start").transform;
        }

        public MovingCube SpawnCube()
        {
            var tmp_Cube = Instantiate(cube, root);
            var tmp_Position = selfTrans.position;
            var tmp_MovingCube = tmp_Cube.GetComponent<MovingCube>();
            tmp_MovingCube.CubeDirection = CubeDirection;
            if (StackARManager.INSTANCE.LastCube && !StackARManager.INSTANCE.LastCube.Equals(startPoint))
            {
                var tmp_LastPoint = StackARManager.INSTANCE.LastCube.transform.position;
                tmp_Cube.transform.position = new Vector3(tmp_LastPoint.x,
                    tmp_LastPoint.y + tmp_Cube.transform.localScale.y,
                    tmp_LastPoint.z);
            }
            else
            {
                tmp_Cube.transform.position = tmp_Position;
            }

            return tmp_MovingCube;
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            if (cube.transform)
                Gizmos.DrawWireCube(transform.position, cube.transform.localScale);
        }
    }

    public enum CubeDirection
    {
        XAxis,
        ZAxis
        
    }
}