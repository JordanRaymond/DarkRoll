using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DR
{
    public class InputHandler : MonoBehaviour
    {
        float vertical;
        float horizontal;

        StateManager states;

        void Start() {
            states = GetComponent<StateManager>();
            states.Init();
        }

        void Update() {
            UpdateStates();
        }

        private void FixedUpdate() {
            GetInput(); // In update?   
        }

        void GetInput() {
            vertical = Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");
        }

        void UpdateStates() {
            states.horizontal = horizontal;
            states.vertical = vertical;

            states.Tick(Time.deltaTime);
        }
    } 

}
