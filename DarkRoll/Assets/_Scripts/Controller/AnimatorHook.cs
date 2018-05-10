using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DR
{
    public class AnimatorHook : MonoBehaviour
    {
        Animator animator;
        StateManager states;

        public void Init(StateManager st)
        {
            states = st;
            animator = st.animator;
        }

        private void OnAnimatorMove()
        {
            if (states.canMove)
                return;

            states.rigid.drag = 0;
            float multiplier = 1;

            Vector3 delta = animator.deltaPosition;
            delta.y = 0;
            Vector3 v = (delta * multiplier) / states.delta;
            states.rigid.velocity = v;
        }
    }

}
