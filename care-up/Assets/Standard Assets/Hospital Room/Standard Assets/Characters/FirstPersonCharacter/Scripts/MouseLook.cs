using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [Serializable]
    public class MouseLook
    {
        [HideInInspector]
        public bool lookOnly = false;

        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public bool clampHorisontalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public float MinimumY = -90F;
        public float MaximumY = 90F;
        public bool smooth;
        public float smoothTime = 5f;
        public bool lockCursor = true;
        
        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;

        public bool savedRot = false;
        private Quaternion savedCamRot;
        private Quaternion savedCharRot;

        public float XTouchSensetivity = 0.2f;
        public float YTouchSensetivity = 0.2f;

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }
        
        public float LookRotation(Transform character, Transform camera)
        {
            float yRot, xRot;

            if (Input.touchCount > 0)
            {
                xRot = Input.GetTouch(0).deltaPosition.x * XTouchSensetivity;
                yRot = Input.GetTouch(0).deltaPosition.y * YTouchSensetivity;
            }
            else
            {
                xRot = Input.GetAxisRaw("Mouse Y") * XSensitivity;
                yRot = Input.GetAxisRaw("Mouse X") * YSensitivity;
            }

            if (lookOnly)
            {
                m_CameraTargetRot *= Quaternion.Euler(-xRot, yRot, 0f);
                m_CameraTargetRot = Quaternion.Euler(m_CameraTargetRot.eulerAngles.x, m_CameraTargetRot.eulerAngles.y, 0f);
            }
            else
            {
                m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
                m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);
            }

            if(clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis (m_CameraTargetRot);
            if(clampHorisontalRotation)
                m_CameraTargetRot = ClampRotationAroundYAxis(m_CameraTargetRot);

            if (lookOnly)
            {
                if (smooth)
                {
                    camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                        smoothTime * Time.deltaTime);
                }
                else
                {
                    camera.localRotation = m_CameraTargetRot;
                }
            }
            else
            {
                if (smooth)
                {
                    character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot,
                        smoothTime * Time.deltaTime);
                    camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                        smoothTime * Time.deltaTime);
                }
                else
                {
                    character.localRotation = m_CharacterTargetRot;
                    camera.localRotation = m_CameraTargetRot;
                }
            }

            //UpdateCursorLock();

            return new Vector2(xRot, yRot).magnitude;
        }

        public void ToggleMode(bool value, Transform character, Transform camera)
        {
            if (value)
            {
                savedCamRot = camera.rotation;
                savedCharRot = character.rotation;
                savedRot = true;
            }
            /*else
            {
                if (savedRot)
                {
                    // this order specifically, parenting
                    character.rotation = savedCharRot;
                    camera.rotation = savedCamRot;
                }
            }*/
        }

        public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if(!lockCursor)
            {//we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void UpdateCursorLock()
        {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

            angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

        Quaternion ClampRotationAroundYAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleY = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);

            angleY = Mathf.Clamp(angleY, MinimumY, MaximumY);

            q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleY);

            return q;
        }

        public void SetMode(bool value, Quaternion cam)
        {
            lookOnly = value;
            if (!value)
            {
                m_CameraTargetRot = cam;
            }
        }
    }
}
