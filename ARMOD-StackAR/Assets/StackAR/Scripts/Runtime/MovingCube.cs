using System;
using com.Phantoms.ActionNotification.Runtime;
using UnityEngine;
using XRMODEngineCore.Runtime;

namespace StackAR.Runtime
{
    public class MovingCube : XRMODBehaviour
    {
        private const float CONST_ERROR_MARGIN = 0.03f;
        private const float CONST_BOUND_SIZE = 2;
        private Transform selfTrans;
        private float cubeTransition;

        public float moveSpeed = 5;
        public CubeDirection CubeDirection;
        public bool stop;

        private void Start()
        {
            selfTrans = this.transform;
            Resize();
        }

        /// <summary>
        /// Continue the size of the previous cube
        /// </summary>
        private void Resize()
        {
            var tmp_LastScale = StackARManager.INSTANCE.LastCube.localScale;
            var tmp_LocalScale = selfTrans.localScale;
            tmp_LocalScale = new Vector3(tmp_LastScale.x, tmp_LocalScale.y, tmp_LastScale.z);
            selfTrans.localScale = tmp_LocalScale;
        }


        private void Update()
        {
            if (stop) return;
            cubeTransition += Time.deltaTime * moveSpeed;
            float tmp_MovePingpong = Mathf.Sin(cubeTransition) * CONST_BOUND_SIZE;
            var tmp_LocalPosition = selfTrans.localPosition;

            switch (CubeDirection)
            {
                case CubeDirection.XAxis:
                    tmp_LocalPosition =
                        new Vector3(tmp_MovePingpong, tmp_LocalPosition.y, tmp_LocalPosition.z);
                    break;
                case CubeDirection.ZAxis:
                    tmp_LocalPosition =
                        new Vector3(tmp_LocalPosition.x, tmp_LocalPosition.y, tmp_MovePingpong);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            selfTrans.localPosition = tmp_LocalPosition;
        }

        /// <summary>
        /// Split the cube and falling
        /// </summary>
        /// <param name="_hangover">The size of the dangling part</param>
        private void SplitCube(float _hangover)
        {
            float tmp_Direction = _hangover > 0 ? 1f : -1f;

            var tmp_SelfScale = selfTrans.localScale;
            var tmp_SelfPosition = selfTrans.localPosition;

            var tmp_LastScale = StackARManager.INSTANCE.LastCube.localScale;
            var tmp_LastPosition = StackARManager.INSTANCE.LastCube.localPosition;


            float tmp_SplitSize = CubeDirection == CubeDirection.XAxis
                ? tmp_LastScale.x - Mathf.Abs(_hangover)
                : tmp_LastScale.z - Mathf.Abs(_hangover);


            float tmp_FallingCubeSize = CubeDirection == CubeDirection.XAxis
                ? tmp_SelfScale.x - tmp_SplitSize
                : tmp_SelfScale.z - tmp_SplitSize;

            tmp_SelfScale = CubeDirection == CubeDirection.XAxis
                ? new Vector3(tmp_SplitSize, tmp_SelfScale.y, tmp_SelfScale.z)
                : new Vector3(tmp_SelfScale.x, tmp_SelfScale.y, tmp_SplitSize);

            selfTrans.localScale = tmp_SelfScale;


            float tmp_NewPosition = CubeDirection == CubeDirection.XAxis
                ? tmp_LastPosition.x + (_hangover / 2)
                : tmp_LastPosition.z + (_hangover / 2);

            tmp_SelfPosition = CubeDirection == CubeDirection.XAxis
                ? new Vector3(tmp_NewPosition, tmp_SelfPosition.y, tmp_SelfPosition.z)
                : new Vector3(tmp_SelfPosition.x, tmp_SelfPosition.y, tmp_NewPosition);

            selfTrans.localPosition = tmp_SelfPosition;


            CreateFallingCube(tmp_SelfPosition, tmp_SelfScale, tmp_SplitSize, tmp_FallingCubeSize, tmp_Direction);
        }


        private void CreateFallingCube(Vector3 _position, Vector3 _scale, float _splitSize, float _fallingCubeSize,
            float _direction)
        {
            float tmp_Edge = (CubeDirection == CubeDirection.XAxis ? _position.x : _position.y) +
                             _splitSize / 2 * _direction;
            float tmp_FallingBlockPosition = tmp_Edge + _fallingCubeSize / 2 * _direction;
            var tmp_FallingCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var tmp_FallingCubeTrans = tmp_FallingCube.transform;
            tmp_FallingCubeTrans.SetParent(selfTrans.parent);

            tmp_FallingCubeTrans.localScale = CubeDirection == CubeDirection.XAxis
                ? new Vector3(_fallingCubeSize, _scale.y, _scale.z)
                : new Vector3(_scale.x, _scale.y, _fallingCubeSize);

            tmp_FallingCubeTrans.localPosition = CubeDirection == CubeDirection.XAxis
                ? new Vector3(tmp_FallingBlockPosition, _position.y, _position.z)
                : new Vector3(_position.x, _position.y, tmp_FallingBlockPosition);

            tmp_FallingCube.AddComponent<Rigidbody>();
            tmp_FallingCube.GetComponent<Renderer>().material = GetComponent<Renderer>().material;
            tmp_FallingCube.GetComponent<MeshFilter>().mesh.colors32 = GetComponent<MeshFilter>().mesh.colors32;
        }


        public void Stop()
        {
            stop = true;
            moveSpeed = 0;

            //How much length needs to be split
            var tmp_LocalPosition = selfTrans.localPosition;
            var tmp_Position = StackARManager.INSTANCE.LastCube.localPosition;
            float tmp_Hangover = CubeDirection == CubeDirection.XAxis
                ? tmp_LocalPosition.x - tmp_Position.x
                : tmp_LocalPosition.z - tmp_Position.z;


            //Is death?
            var tmp_LocalScale = StackARManager.INSTANCE.LastCube.localScale;
            var tmp_MaxSize = CubeDirection == CubeDirection.XAxis ? tmp_LocalScale.x : tmp_LocalScale.z;
            if (Mathf.Abs(tmp_Hangover) >= tmp_MaxSize)
            {
                this.gameObject.AddComponent<Rigidbody>();
                // StackARManager.INSTANCE.gameState = StackARManager.GameState.Over;
                ActionNotificationCenter.DefaultCenter.PostNotification("UpdateGameState",
                    new GameStateNotificationData() {GameState = GameState.Over});

                return;
            }


            if (Mathf.Abs(tmp_Hangover) < CONST_ERROR_MARGIN)
            {
                //Perfect stacking!!!
                AlignToLast();
                StackARManager.INSTANCE.gameScoreNotificationData.UpdatedScore = 2;
            }
            else
            {
                SplitCube(tmp_Hangover);
                StackARManager.INSTANCE.gameScoreNotificationData.UpdatedScore = 1;
            }

            ActionNotificationCenter.DefaultCenter.PostNotification("UpdateScore",
                StackARManager.INSTANCE.gameScoreNotificationData);

            StackARManager.INSTANCE.LastCube = this.transform;
            this.enabled = false;
        }

        private void AlignToLast()
        {
            var tmp_LocalPosition = StackARManager.INSTANCE.LastCube.localPosition;
            selfTrans.localPosition = new Vector3(tmp_LocalPosition.x,
                selfTrans.localPosition.y, tmp_LocalPosition.z);
        }

        private void OnDestroy()
        {
            Destroy(this);
        }
    }
}