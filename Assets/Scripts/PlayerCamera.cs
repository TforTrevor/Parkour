using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Parkour
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] BoolVariable enableLook;
        [SerializeField] Vector2Variable lookInput;
        [SerializeField] FloatVariable sensitivity;
        [SerializeField] float maxPitch;
        [SerializeField] float minPitch;        
        [SerializeField] float wallRideRoll;

        float pitch;
        float roll;

        void Start()
        {
            LockCursor();
        }

        void Update()
        {
            if (enableLook.Value)
            {
                pitch -= lookInput.Value.y * sensitivity.Value * 0.01f;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                transform.localEulerAngles = new Vector3(pitch, 0, roll);
            }            
        }

        //Item2: true is rotate right, false is rotate left
        public void WallRide(BoolPair pair)
        {
            if (pair.Item1)
            {
                float amount = pair.Item2 ? -wallRideRoll : wallRideRoll;
                DOTween.To(() => roll, x => roll = x, amount, 0.5f);
            }
            else
            {
                DOTween.To(() => roll, x => roll = x, 0, 0.5f);
            }
        }

        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}