using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Parkour
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] Vector2Variable lookInput;
        [SerializeField] float maxPitch;
        [SerializeField] float minPitch;
        [SerializeField] float sensitivity;
        [SerializeField] float wallRideRoll;

        float pitch;
        float roll;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            pitch -= lookInput.Value.y * sensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            transform.localEulerAngles = new Vector3(pitch, 0, roll);
        }

        //True is rotate right, false is rotate left
        public void WallRide(bool value, bool direction)
        {
            if (value)
            {
                float amount = direction ? -wallRideRoll : wallRideRoll;
                DOTween.To(() => roll, x => roll = x, amount, 0.5f);
            }
            else
            {
                DOTween.To(() => roll, x => roll = x, 0, 0.5f);
                //roll = 0;
            }
        }
    }
}