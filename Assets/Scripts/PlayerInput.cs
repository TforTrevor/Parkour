using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Parkour
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] Vector2Variable move;
        [SerializeField] Vector2Variable look;
        [SerializeField] VoidEvent openSettings;
        [SerializeField] VoidEvent closeSettings;

        PlayerMovement movement;
        UnityEngine.InputSystem.PlayerInput input;
        bool settingsOpen;

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
                case "Settings":
                    if (action.ReadValue<float>() == 1)
                    {
                        settingsOpen = !settingsOpen;
                        if (settingsOpen)
                            openSettings.Raise();
                        else
                            closeSettings.Raise();
                    }                    
                    break;
                case "Crouch":
                    if (action.ReadValue<float>() == 1)
                        movement.Crouch(true);
                    else
                        movement.Crouch(false);
                    break;
                case "Sprint":
                    if (action.ReadValue<float>() == 1)
                        movement.ToggleSprint(true);
                    break;

            }            
        }
    }
}