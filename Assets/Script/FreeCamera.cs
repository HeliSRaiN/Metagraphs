﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace nm
{
    public class FreeCamera : MonoBehaviour
    {
        [Range(0f, 100f)]
        public float moveSpeed = 5f;
        [Range(0f, 100f)]
        public float sprintSpeed = 15f;
        [Range(0f, 1f)]
        public float mouseTurnSpeed = 0.5f;
        [Range(0f, 1f)]
        public float smoothness = 0.36f;

        public float dragSpeed = 6f;

        public Transform compass;
        public Transform compassObject;

        [HideInInspector] public bool m_inputCaptured;
        [HideInInspector] public bool m_rotateAroud;
        float m_yaw, m_pitch, speed, forward, right, up;

        public Vector3 offset;
        public float zoom = 0.2f;
        public float zoomMax = 10;
        public float zoomMin = 3;

        public GameObject menu;
        [HideInInspector] public string selectedObject = null;
        public GameObject rotAObject;
        public GameObject scrollBarRotA;
        public Transform CenterRotMarker;

        private Scrollbar speedRotate;
        private Vector3 targetPosition;
        private Quaternion standartZero = Quaternion.Euler(Vector3.zero);

        private InteractionModule interactionM;

        private Quaternion rotation;
        private EditorMenu editorMenu;

        private GUIStyle fpsStyle = new GUIStyle();

        private void Start()
        {
            fpsStyle.alignment = TextAnchor.MiddleLeft;

            editorMenu = menu.GetComponent<EditorMenu>();
            interactionM = InteractionModule.GetInit();
            offset = new Vector3(offset.x, offset.y, Mathf.Abs(zoomMin));
            speedRotate = scrollBarRotA.GetComponent<Scrollbar>();
        }

        public void UpdateMouseSetting()
        {
            mouseTurnSpeed = editorMenu.mouseSensitivity;
            smoothness = editorMenu.smoothingMotion;
        }

        // Вернули управление.
        void CaptureInput()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            m_inputCaptured = true;

            m_yaw = transform.eulerAngles.y;
            m_pitch = transform.eulerAngles.x;
        }

        // Забрали управление.
        void ReleaseInput()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            m_inputCaptured = false;
        }

        void OnApplicationFocus(bool focus)
        {
            if (m_inputCaptured && !focus)
                ReleaseInput();
        }

        public void SetAlignment()
        {
            Vector3 currentRotation = Camera.main.transform.localRotation.eulerAngles;
            Camera.main.transform.localRotation = Quaternion.Euler(Vector3.up);
        }

        public Texture2D cursor1;

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f)
                angle += 360f;
            if (angle > 360f)
                angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }

        private void FixedUpdate()
        {
            compassObject.rotation = Quaternion.Euler(transform.eulerAngles.x, 0, 0);
            compass.localRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        }

        public float zoomSpeed = 2f;

        void Update()
        {

            // Если у нас управление камерой и мы не крутимся.
            if (!m_inputCaptured && !m_rotateAroud)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // Если перед камерой объект.
                if (Physics.Raycast(ray, out hit))
                {
                    //if (hit.collider.tag == "Metagraph")
                    //{
                    Transform objectHit = hit.collider.transform;

                    if (Input.GetMouseButtonDown(0))
                    {
                        // Если луч указывает на UI элемент, то показания луча недействительны.
                        if (EventSystem.current.IsPointerOverGameObject()) return;
                        // Если элемент существует
                        if (interactionM.IsExitObjectInStructure(objectHit.name))
                        {
                            if (interactionM.isConnection)
                            {
                                selectedObject = objectHit.name;
                                interactionM.EndConnection();
                            }
                            else
                            {
                                interactionM.SelectObjectAndOpenMenu(objectHit.name);
                            }
                        }
                        interactionM.DoubleTap("DoubleTapToObject");
                    }
                    //}
                }
                else
                {
                    // Если луч указывает на UI элемент, то показания луча недействительны.
                    if (EventSystem.current.IsPointerOverGameObject()) return;

                    if (Input.GetMouseButtonDown(0))
                    {
                        interactionM.DoubleTap("DoubleTapToNull");
                    }
                }
            }

            // Если меню не активно.
            if (!editorMenu.menuActive/* && !changeM.isPanelActive*/)
            {
                //Right Mouse
                if (Input.GetMouseButtonDown(1))
                {
                    if (!m_rotateAroud)
                    {
                        if (!m_inputCaptured)
                        {
                            CaptureInput();
                        }
                        else if (m_inputCaptured)
                        {
                            ReleaseInput();
                        }
                    }
                }

                // При нажатии показываем маркер центра.
                if (Input.GetKeyDown(EditorMenu.keys[7]))
                {
                    CenterRotMarker.gameObject.SetActive(true);
                }

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    //Left Mouse
                    if (Input.GetMouseButtonDown(0))
                    {
                        Cursor.SetCursor(cursor1, Vector2.zero, CursorMode.Auto);
                    }
                    //Left Mouse
                    if (Input.GetMouseButton(0))
                    {
                        transform.Translate(-Input.GetAxis("Mouse X") * dragSpeed, -Input.GetAxisRaw("Mouse Y") * dragSpeed, 0);
                    }
                    //Left Mouse
                    if (Input.GetMouseButtonUp(0))
                    {
                        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    }
                }

                // При вращении или удерживании кнопки можно настроить дальность.
                if (m_rotateAroud || Input.GetKey(EditorMenu.keys[7]))
                {
                    CenterRotMarker.position = targetPosition;
                    CenterRotMarker.rotation = standartZero;

                    if (Input.GetAxis("Mouse ScrollWheel") < 0)
                    {
                        offset.z += zoom;
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") > 0)
                    {
                        offset.z -= zoom;
                    }
                    offset.z = Mathf.Clamp(offset.z, Mathf.Abs(zoomMin), float.MaxValue);
                }
                else
                {
                    // Приближение и удаление с помощью колёсика
                    transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, Space.Self);
                }

                // В момент отпускания мы убираем метку центра.
                if (Input.GetKeyUp(EditorMenu.keys[7]))
                {
                    CenterRotMarker.gameObject.SetActive(false);
                    if (!m_inputCaptured)
                    {
                        m_rotateAroud = (m_rotateAroud) ? false : true;
                        rotAObject.SetActive(m_rotateAroud);
                    }
                }
            }

            if (m_rotateAroud)
            {
                transform.Rotate(Vector3.up * Time.deltaTime * 100 * speedRotate.value);
                transform.position = transform.localRotation * -offset + targetPosition;
            }
            else
            {
                targetPosition = transform.position + transform.localRotation * offset;
            }

            if (!m_inputCaptured)
                return;

            // Вращение камеры
            m_yaw = (m_yaw + mouseTurnSpeed * 10 * Input.GetAxis("Mouse X")) % 360f;
            m_pitch = ClampAngle((m_pitch - mouseTurnSpeed * 10 * Input.GetAxis("Mouse Y")) % 360f, -90f, 90f);
            rotation = Quaternion.AngleAxis(m_yaw, Vector3.up) * Quaternion.AngleAxis(m_pitch, Vector3.right);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, smoothness);

            // Перемещение камеры
            speed = Time.deltaTime * (Input.GetKey(EditorMenu.keys[6]) ? sprintSpeed : moveSpeed);
            right = speed * ((Input.GetKey(EditorMenu.keys[1]) ? 1f : 0f) - (Input.GetKey(EditorMenu.keys[0]) ? 1f : 0f));
            forward = speed * ((Input.GetKey(EditorMenu.keys[2]) ? 1f : 0f) - (Input.GetKey(EditorMenu.keys[3]) ? 1f : 0f));

            up = speed * ((Input.GetKey(EditorMenu.keys[4]) ? 1f : 0f) - (Input.GetKey(EditorMenu.keys[5]) ? 1f : 0f));
            transform.position += transform.forward * forward + transform.right * right + Vector3.up * up;
        }

    }
}
