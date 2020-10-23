using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using Cinemachine;

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
        [SerializeField] float normalHeight;
        [SerializeField] float crouchHeight;
        [SerializeField] float crouchTime;

        CinemachineVirtualCamera virtualCamera;
        float pitch;
        float roll;

        void Awake()
        {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        void Start()
        {
            LockCursor();
        }

        void Update()
        {
            if (enableLook.Value)
            {
                pitch -= lookInput.Value.y * sensitivity.Value * 0.01f * Time.timeScale;
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

        public void SetFOV(int value)
        {
            float vertical = Camera.HorizontalToVerticalFieldOfView(value, virtualCamera.m_Lens.Aspect);
            virtualCamera.m_Lens.FieldOfView = vertical;
        }

        public void Crouch(bool value)
        {
            if (value)
            {
                DOTween.To(() => transform.localPosition, x => transform.localPosition = x, new Vector3(0, crouchHeight, 0), crouchTime);
            }
            else
            {
                DOTween.To(() => transform.localPosition, x => transform.localPosition = x, new Vector3(0, normalHeight, 0), crouchTime);
            }
        }
    }
}