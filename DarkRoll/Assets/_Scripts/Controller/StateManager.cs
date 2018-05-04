using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DR
{
    public class StateManager : MonoBehaviour
    {
        public float horizontal;
        public float vertical;

        public GameObject activeModel;
        [HideInInspector] public Animator animator;
        [HideInInspector] public Rigidbody rigid;

        [HideInInspector] public float delat;

        public void Init() {
            SetupAnimator();

            rigid = GetComponent<Rigidbody>();
        }

        void SetupAnimator() {
            if (activeModel != null) {
                animator = activeModel.GetComponent<Animator>();
            }
            else {
                // I asume this script is on a controller game object and the animator is on a child obj
                animator = GetComponentInChildren<Animator>();

                if (animator == null) {
                    Debug.LogError("No active model found, be sure to add the active model in the inspector. The active model should have an animator. " +
                        "If no animator is found, the script will check for one in the child.");
                }
                else {
                    activeModel = animator.gameObject;
                }
            }

            animator.applyRootMotion = false;
        }

        public void Tick(float delta) {

        }
    }

}
