using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DR
{
    public class CameraManager : MonoBehaviour
    {

        public static CameraManager singleton;

        public bool lockOn;
        public float followSpeed = 9;
        public float mouseSpeed = 2;
        public float controllerSpeed = 7;

        public Transform target;
        public EnemyTarget lockOnTarget;
        public Transform lockOnTransform;

        [HideInInspector] public Transform pivot;
        [HideInInspector] public Transform camTrans;
        StateManager states;

        float turnSmoothing = 0.1f;
        public float minAngle = -35f;
        public float maxAngle = 35f;

        float smoothX;
        float smoothY;
        float smoothXVelocity;
        float smoothYVelocity;
        [SerializeField] float lookAngle;
        [SerializeField] float tiltAngle;

        bool useRightAxis;

        private void Awake()
        {
            if (singleton != null)
            {
                Destroy(gameObject);
            }

            singleton = this;
        }

        public void Init(StateManager state)
        {
            states = state;
            target = state.transform;

            camTrans = Camera.main.transform;
            pivot = camTrans.parent;
        }

        public void Tick(float delta)
        {
            float h = Input.GetAxis("Mouse X");
            float v = Input.GetAxis("Mouse Y");

            float cntrl_H = Input.GetAxis("RightAxis X");
            float cntrl_V = Input.GetAxis("RightAxis Y");

            float targetSpeed = mouseSpeed;

            if (lockOnTarget != null)
            {
                if (lockOnTransform == null)
                {
                    lockOnTransform = lockOnTarget.GetTarget();
                    states.lockOnTransform = lockOnTransform;
                }

                if (Mathf.Abs(cntrl_H) > 0.6f)
                {
                    if (!useRightAxis)
                    {
                        lockOnTransform = lockOnTarget.GetTarget(cntrl_H > 0);
                        states.lockOnTransform = lockOnTransform;
                        useRightAxis = true;
                    }
                }
            }

            if (useRightAxis)
            {
                if(Mathf.Abs(cntrl_H) < 0.6f)
                {
                    useRightAxis = false;
                }
            }

            if (cntrl_H != 0 || cntrl_V != 0)
            {
                h = cntrl_H;
                v = cntrl_V;

                targetSpeed = controllerSpeed;
            }

            FollowTarget(delta);
            HandleRotation(delta, h, v, targetSpeed);
        }

        void FollowTarget(float d)
        {
            float speed = d * followSpeed;

            Vector3 targetPosition = Vector3.Lerp(transform.position, target.position, speed);
            transform.position = targetPosition;
        }

        void HandleRotation(float d, float h, float v, float targetSpeed)
        {
            if (turnSmoothing > 0)
            {
                smoothX = Mathf.SmoothDamp(smoothX, h, ref smoothXVelocity, turnSmoothing);
                smoothY = Mathf.SmoothDamp(smoothY, v, ref smoothYVelocity, turnSmoothing);
            } else
            {
                smoothX = h;
                smoothY = v;
            }

            tiltAngle -= smoothY * targetSpeed;
            tiltAngle = Mathf.Clamp(tiltAngle, minAngle, maxAngle);

            pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);
            lookAngle += smoothX * targetSpeed;

            if (lockOn && lockOnTarget != null)
            {
                Vector3 targetDir = lockOnTransform.position - transform.position;
                targetDir.Normalize();
                targetDir.y = 0;

                if (targetDir == Vector3.zero)
                    targetDir = transform.forward;

                Quaternion targetRot = Quaternion.LookRotation(targetDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, d * 9);
                lookAngle = transform.eulerAngles.y;

                return;
            }

            transform.rotation = Quaternion.Euler(0, lookAngle, 0);
        }
    }

}