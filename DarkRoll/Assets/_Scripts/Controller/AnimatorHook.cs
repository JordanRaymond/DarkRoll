using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DR
{
    public class AnimatorHook : MonoBehaviour
    {
        Animator animator;
        StateManager states;

        // rootMotionMultiplier
        public float rm_multiplier;
        bool rolling;
        float rollT;

        public void Init(StateManager st)
        {
            states = st;
            animator = st.animator;
        }

        public void InitForRoll()
        {
            rolling = true;
            rollT = 0;
        }

        public void CloseRoll()
        {
            if (rolling == false)
                return;

            rm_multiplier = 1;
            rollT = 0;
            rolling = false;
        }

        private void OnAnimatorMove()
        {
            if (states.canMove)
                return;

            states.rigid.drag = 0;

            if (rm_multiplier == 0)
                rm_multiplier = 1;

            if (rolling == false)
            {
                Vector3 delta = animator.deltaPosition;
                delta.y = 0;
                Vector3 v = (delta * rm_multiplier) / states.delta;
                states.rigid.velocity = v; 
            }
            else
            {
                rollT += states.delta / .6f;

                if (rollT > 1)
                    rollT = 1;

                float zValue = states.rollCurve.Evaluate(rollT);
                Vector3 v1 = Vector3.forward * zValue;
                Vector3 relative = transform.TransformDirection(v1);
                Vector3 v2 = (relative * rm_multiplier);

                states.rigid.velocity = v2;
            }
        }
    }

}
