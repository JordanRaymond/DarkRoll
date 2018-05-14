using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DR
{
    public class InputHandler : MonoBehaviour
    {
        float vertical;
        float horizontal;
        bool b_input;
        bool a_input;
        bool x_input;
        bool y_input;

        bool rb_input;
        float rt_axis;
        bool rt_input;
        bool lb_input;
        float lt_axis;
        bool lt_input;

        bool leftAxis_down;
        bool rightAxis_down;

        StateManager states;
        CameraManager cameraManager;

        float delta; 

        void Start() {
            states = GetComponent<StateManager>();
            states.Init();

            cameraManager = CameraManager.singleton;
            cameraManager.Init(states);
        }

        void Update()
        {
            delta = Time.deltaTime;
            states.Tick(delta);
        }

        private void FixedUpdate() {
            delta = Time.fixedDeltaTime;

            GetInput(); // In update?   
            UpdateStates();

            states.FixedTick(delta);
            cameraManager.Tick(delta);
        }


        void GetInput() {
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
            b_input = Input.GetButton("B");
            a_input = Input.GetButton("A");
            x_input = Input.GetButton("X");
            y_input = Input.GetButtonUp("Y");

            rt_input = Input.GetButton("RT");
            rt_axis = Input.GetAxis("RT");
            if (rt_axis != 0)
                rt_input = true;

            lt_input = Input.GetButton("LT");
            lt_axis = Input.GetAxis("LT");
            if (lt_axis != 0)
                lt_input = true;

            rb_input = Input.GetButton("RB");
            lb_input = Input.GetButton("LB");

            rightAxis_down = Input.GetButtonUp("L");
        }

        void UpdateStates() {
            states.horizontal = horizontal;
            states.vertical = vertical;

            Vector3 v = vertical * cameraManager.transform.forward;
            Vector3 h = horizontal * cameraManager.transform.right;
            states.moveDir = (v + h).normalized;
            float m = Mathf.Abs(horizontal) + Mathf.Abs(vertical);

            states.moveAmount = Mathf.Clamp01(m);

            states.rollInput = b_input;

            // states.isRunning = (b_input && (states.moveAmount > 0));

            states.rt = rt_input;
            states.rb = rb_input;
            states.lb = lb_input;
            states.lt = lt_input;

            if (y_input)
            {
                states.isTwoHanded = !states.isTwoHanded;
                states.HangleTwoHanded();
            }

            if (rightAxis_down)
            {
                states.isLockOn = !states.isLockOn;

                if (states.lockOnTarget == null)
                    states.isLockOn = false;

                cameraManager.lockOnTarget = states.lockOnTarget;
                states.lockOnTransform = cameraManager.lockOnTransform;
                cameraManager.lockOn = states.isLockOn;
            }
        }
    } 

}
