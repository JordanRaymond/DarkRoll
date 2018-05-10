using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DR
{
    public class StateManager : MonoBehaviour
    {
        [Header("Init")]
        public GameObject activeModel;

        [Header("Inputs")]
        public float horizontal;
        public float vertical;
        public float moveAmount;
        public Vector3 moveDir;
        public bool rt, rb, lt, lb;


        [Header("Stats")]
        public float moveSpeed = 2;
        public float runSpeed = 3.5f;
        public float rotationSpeed = 5f;
        public float toGround = 0.5f;

        [Header("States")]
        public bool isRunning = false;
        public bool isOnGround = false;
        public bool isLockON = false;
        public bool isInAction = false;
        public bool canMove = false;
        public bool isTwoHanded = false;

        [HideInInspector] public Animator animator;
        [HideInInspector] public Rigidbody rigid;
        [HideInInspector] public AnimatorHook a_hook;


        [HideInInspector] public float delta;

        [SerializeField] LayerMask ignoreLayers;

        float _actionDelay;

        public void Init()
        {
            SetupAnimator();

            rigid = GetComponent<Rigidbody>();
            rigid.angularDrag = 999;
            rigid.drag = 4;

            rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            a_hook = activeModel.AddComponent<AnimatorHook>();
            a_hook.Init(this);

            gameObject.layer = 9;
            ignoreLayers = ~(1 << 10);

            animator.SetBool("isOnGround", true);
        }

        void SetupAnimator()
        {
            if (activeModel != null)
            {
                animator = activeModel.GetComponent<Animator>();
            }
            else
            {
                // I asume this script is on a controller game object and the animator is on a child obj
                animator = GetComponentInChildren<Animator>();

                if (animator == null)
                {
                    Debug.LogError("No active model found, be sure to add the active model in the inspector. The active model should have an animator. " +
                        "If no animator is found, the script will check for one in the child.");
                }
                else
                {
                    activeModel = animator.gameObject;
                }
            }

            animator.applyRootMotion = false;
        }

        public void Tick(float p_delta)
        {
            delta = p_delta;
            isOnGround = OnGround();

            animator.SetBool("isOnGround", isOnGround);
        }

        public void FixedTick(float p_delta)
        {
            delta = p_delta;

            DetectAction();

            if (isInAction)
            {
                animator.applyRootMotion = true;

                _actionDelay += delta; 
                if(_actionDelay > 0.3f)
                {
                    isInAction = false;
                    _actionDelay = 0;
                } else
                {
                    return;
                }
            }
                            
            canMove = animator.GetBool("canMove");

            if (!canMove)
                return;

            animator.applyRootMotion = false;

            rigid.drag = (moveAmount > 0 || !isOnGround) ? 0 : 4;

            float targetSpeed = moveSpeed;
            if (isRunning)
                targetSpeed = runSpeed;

            if (isOnGround)
                rigid.velocity = moveDir * (targetSpeed * moveAmount);

            if (isRunning) isLockON = false;


            if (isOnGround)
            {
                Vector3 targetDir = moveDir;
                targetDir.y = 0;
                if (targetDir == Vector3.zero)
                {
                    targetDir = transform.forward;
                }

                Quaternion tr = Quaternion.LookRotation(targetDir);
                Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, delta * moveAmount * rotationSpeed);
                transform.rotation = targetRotation;
            }

            HangleMovementAnimations();
        }

        public void DetectAction()
        {
            if (!canMove)
                return;

            if (!rb && !rt && !lt && !lb)
                return;

            string targetAnim = null;

            if (rb)
                targetAnim = "oh_attack_1";
            if (rt)
                targetAnim = "oh_attack_2";
            if (lt)
                targetAnim = "oh_attack_3";
            if (lb)
                targetAnim = "th_attack_1";

            if (string.IsNullOrEmpty(targetAnim))
                return;
            
            canMove = false;
            isInAction = true;
            animator.CrossFade(targetAnim, 0.2f);
        }

        void HangleMovementAnimations()
        {
            animator.SetBool("isRunning", isRunning);
            animator.SetFloat("vertical", moveAmount, 0.4f, delta);
        }

        public bool OnGround()
        {
            bool r = false;

            Vector3 origin = transform.position + (Vector3.up * toGround);
            Vector3 dir = Vector3.down;
            float dis = toGround + 0.3f;

            RaycastHit hit;
            if (Physics.Raycast(origin, dir, out hit, dis, ignoreLayers))
            {
                r = true;
                Vector3 targetPosition = hit.point;
                transform.position = targetPosition;
            }

            return r;
        }

        public void HangleTwoHanded()
        {
            animator.SetBool("twoHanded", isTwoHanded);
        }
    }

}
