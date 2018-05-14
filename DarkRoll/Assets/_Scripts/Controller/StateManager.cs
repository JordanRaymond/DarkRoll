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
        public bool rollInput;

        [Header("Stats")]
        public float moveSpeed = 2;
        public float runSpeed = 3.5f;
        public float rotationSpeed = 5f;
        public float toGround = 0.5f;
        public float rollSpeed = 1;

        [Header("States")]
        public bool isRunning = false;
        public bool isOnGround = false;
        public bool isLockOn = false;
        public bool isInAction = false;
        public bool canMove = false;
        public bool isTwoHanded = false;

        [Header("Other")]
        public EnemyTarget lockOnTarget;
        public Transform lockOnTransform;
        public AnimationCurve rollCurve;

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
                if (_actionDelay > 0.3f)
                {
                    isInAction = false;
                    _actionDelay = 0;
                }
                else
                {
                    return;
                }
            }

            canMove = animator.GetBool("canMove");

            if (!canMove)
                return;

            // a_hook.rm_multiplier = 1;
            a_hook.CloseRoll();
            HandleRolls();

            animator.applyRootMotion = false;

            rigid.drag = (moveAmount > 0 || !isOnGround) ? 0 : 4;

            float targetSpeed = moveSpeed;
            if (isRunning)
                targetSpeed = runSpeed;

            if (isOnGround)
                rigid.velocity = moveDir * (targetSpeed * moveAmount);

            if (isRunning) isLockOn = false;

            Vector3 targetDir = (isLockOn == false) ?
                moveDir
                :
                (lockOnTransform != null) ? 
                lockOnTransform.transform.position - transform.position
                :
                moveDir;

            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, delta * moveAmount * rotationSpeed);
            transform.rotation = targetRotation;

            animator.SetBool("lockOn", isLockOn);

            if (!isLockOn)
                HandleMovementAnimations();
            else
                HandleLockOnAnimations(moveDir);
        }

        void HandleRolls()
        {
            if (!rollInput) return;

            float v = vertical;
            float h = horizontal;

            v = (moveAmount > 0.3f) ? 1 : 0;
            h = 0;

            //if(!isLockOn)
            //{
            //    v = (moveAmount > 0.3f) ? 1 : 0;
            //    h = 0;
            //} else
            //{
            //    if (Mathf.Abs(v) < 0.3f)
            //        v = 0;
            //    if (Mathf.Abs(h) < 0.3f)
            //        h = 0;
            //}

            if(v != 0)
            {
                if (moveDir == Vector3.zero)
                    moveDir = transform.forward;

                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = targetRot;
                a_hook.InitForRoll();
                a_hook.rm_multiplier = rollSpeed;
            } else
            {
                a_hook.rm_multiplier = 1.3f;
            }

            animator.SetFloat("vertical", v);
            animator.SetFloat("horizontal", h);

            canMove = false;
            isInAction = true;
            animator.CrossFade("Rolls", 0.2f);
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

        void HandleMovementAnimations()
        {
            animator.SetBool("isRunning", isRunning);
            animator.SetFloat("vertical", moveAmount, 0.4f, delta);
        }

        void HandleLockOnAnimations(Vector3 moveDir)
        {
            Vector3 relativeDir = transform.InverseTransformDirection(moveDir);
            float h = relativeDir.x;
            float v = relativeDir.z;

            animator.SetFloat("vertical", v, 0.2f, delta);
            animator.SetFloat("horizontal", h, 0.2f, delta);
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
