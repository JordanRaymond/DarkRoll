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

        [HideInInspector] public Transform pivot;
        [HideInInspector] public Transform camTrans;

        float turnSmoothing = 0.1f;
        public float minAngle = -35f;
        public float maxAngle = 35f;

        float smoothX;
        float smoothY;
        float smoothXVelocity;
        float smoothYVelocity;
        [SerializeField] float lookAngle;
        [SerializeField] float tiltAngle;

        private void Awake()
        {
            if (singleton != null)
            {
                Destroy(gameObject);
            }

            singleton = this;
        }

        public void Init(Transform p_target)
        {
            target = p_target;

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

            if (lockOn)
            {

            }

            lookAngle += smoothX * targetSpeed;
            transform.rotation = Quaternion.Euler(0, lookAngle, 0);

            tiltAngle -= smoothY * targetSpeed;
            tiltAngle = Mathf.Clamp(tiltAngle, minAngle, maxAngle);

            pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);

        }
    }

}