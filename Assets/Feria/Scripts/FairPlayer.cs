using UnityEngine;
using UnityEngine.InputSystem;

namespace Feria
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FairPlayer : MonoBehaviour
    {
        public Camera view;
        public Transform broom;
        public Transform cloth;
        public FairGame game;
        public float sensitivity = .095f;
        CharacterController controller;
        float pitch = 25f;
        float verticalSpeed;
        Vector3 broomRest;
        public bool Active => game != null && !game.Paused;
        void Awake()
        {
            controller = GetComponent<CharacterController>();
            broomRest = broom.localPosition;
            sensitivity = PlayerPrefs.GetFloat("feria.sensitivity", .095f);
        }
        void Update()
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            if (kb == null || mouse == null) return;
            if (kb.escapeKey.wasPressedThisFrame) game.TogglePause();
            if (game.Paused && kb.enterKey.wasPressedThisFrame) game.SetPaused(false);
            if (!Active) return;
            var delta = mouse.delta.ReadValue() * sensitivity;
            transform.Rotate(0, delta.x, 0);
            pitch = Mathf.Clamp(pitch - delta.y, -55, 78);
            view.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            Vector2 movement = Vector2.zero;
            if (kb.wKey.isPressed) movement.y++;
            if (kb.sKey.isPressed) movement.y--;
            if (kb.dKey.isPressed) movement.x++;
            if (kb.aKey.isPressed) movement.x--;
            movement = Vector2.ClampMagnitude(movement, 1);
            verticalSpeed = controller.isGrounded ? -2 : verticalSpeed - 18 * Time.deltaTime;
            controller.Move(((transform.right * movement.x + transform.forward * movement.y) * (game.MovingCart ? 2.6f : 3.7f) + Vector3.up * verticalSpeed) * Time.deltaTime);
            var p = transform.position;
            p.x = Mathf.Clamp(p.x, -6.5f, 6.5f); p.z = Mathf.Clamp(p.z, -8.3f, 7.6f);
            transform.position = p;
            if (kb.digit1Key.wasPressedThisFrame) game.Tool = 0;
            if (kb.digit2Key.wasPressedThisFrame) game.Tool = 1;
            if (mouse.scroll.ReadValue().y != 0) game.Tool = 1 - game.Tool;
            if (kb.eKey.wasPressedThisFrame) game.Interact();
            if (kb.f5Key.wasPressedThisFrame) game.Save();
            if (kb.tabKey.wasPressedThisFrame) game.RequestHint();
            bool working = mouse.leftButton.isPressed && !game.MovingCart;
            game.UseTool(working);
            float swing = working ? Mathf.Sin(Time.time * 12) : 0;
            broom.localPosition = Vector3.Lerp(broom.localPosition, broomRest + new Vector3(swing * .09f, Mathf.Abs(swing) * .025f, 0), Time.deltaTime * 14);
            broom.localRotation = Quaternion.Euler(0, 0, working ? swing * 7 : 0);
            broom.gameObject.SetActive(game.Tool == 0 && !game.MovingCart);
            if (cloth != null)
            {
                cloth.gameObject.SetActive(game.Tool == 1 && !game.MovingCart);
                cloth.localPosition = new Vector3(.42f + swing * .04f, -.34f, .65f);
                cloth.localRotation = Quaternion.Euler(-12, -15, swing * 8);
            }
        }
        public void Place(Vector3 position, float yaw)
        {
            if (controller == null) controller = GetComponent<CharacterController>();
            controller.enabled = false; transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0)); controller.enabled = true;
        }
    }
}
