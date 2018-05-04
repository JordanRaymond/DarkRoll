using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DR
{
    public class Helper : MonoBehaviour
    {
        [Range(-1, 1)]
        public float vertical;
        [Range(-1, 1)]
        public float horizontal;

        public string[] ohAttacks;
        public string[] thAttacks;

        public bool playAnim;
        public bool twoHanded;
        public bool enableRootMotion;
        public bool useItem;
        public bool interacting;
        public bool lockOn;

        Animator anim; 

        void Start() {
            anim = GetComponent<Animator>();
        }

        void Update() {
            enableRootMotion = !anim.GetBool("canMove");
            anim.applyRootMotion = enableRootMotion;

            interacting = anim.GetBool("interacting");

            if (! lockOn) {
                horizontal = 0;
                vertical = Mathf.Clamp01(vertical);
            }

            anim.SetBool("lockon", lockOn);

            if (enableRootMotion) return;

            if (useItem) {
                anim.Play("use_item");
                useItem = false;
            }

            if (interacting) {
                playAnim = false;
                vertical = Mathf.Clamp(vertical, -0.5f, 0.5f);
            }

            anim.SetBool("twoHanded", twoHanded);

            if (playAnim) {
                string targetAnim;

                if (! twoHanded) {
                    int r = Random.Range(0, ohAttacks.Length);
                    targetAnim = ohAttacks[r];
                } else {
                    int r = Random.Range(0, thAttacks.Length);
                    targetAnim = thAttacks[r];
                }

                if (vertical > 0.5f) {
                    targetAnim = "oh_attack_3";
                }

                vertical = 0;

                anim.CrossFade(targetAnim, 0.2f);
                //anim.SetBool("canMove", false);
                //enableRootMotion = true;
                playAnim = false;
            }

            anim.SetFloat("vertical", vertical);
            anim.SetFloat("horizontal", horizontal);
        }
    }

}