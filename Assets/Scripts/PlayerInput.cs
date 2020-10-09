using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Parkour
{
    public class PlayerInput : MonoBehaviour
    {
        UnityEngine.InputSystem.PlayerInput input;
        [SerializeField] Vector2Variable move;
        [SerializeField] Vector2Variable look;
        PlayerMovement movement;

        void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            input = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        }

        void Start()
        {
            input.onActionTriggered += Callback;
        }

        void Callback(InputAction.CallbackContext context)
        {
            InputAction action = context.action;
            switch (action.name)
            {
                case "Move":
                    move.Value = action.ReadValue<Vector2>();
                    break;
                case "Look":
                    look.Value = action.ReadValue<Vector2>();
                    break;
                case "Jump":
                    if (action.ReadValue<float>() == 1)
                        movement.Jump(true);
                    break;
            }            
        }
    }
}