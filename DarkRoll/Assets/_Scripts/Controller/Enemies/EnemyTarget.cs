using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DR
{
    public class EnemyTarget : MonoBehaviour
    {
        public int index;
        public List<Transform> targets = new List<Transform>();
        public List<HumanBodyBones> humanBodyBones = new List<HumanBodyBones>();

        Animator animator;

        void Start()
        {
            animator = GetComponent<Animator>();
            if (animator.isHuman == false)
                return;

            for (int i = 0; i < humanBodyBones.Count; i++)
            {
                targets.Add(animator.GetBoneTransform(humanBodyBones[i]));
            }
        }

        public Transform GetTarget(bool negative = false)
        {
            if (targets.Count == 0)
                return transform;

            if(negative == false)
            {
                if (index < targets.Count - 1)
                    index++;
                else
                    index = 0;
            }
            else
            {
                if (index < 0)
                    index = targets.Count - 1;
                else
                    index--;
            }

            index = Mathf.Clamp(index, 0, targets.Count - 1);

            return targets[index];
        }

        void Update()
        {

        }
    } 

}
