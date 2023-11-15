using MischievousByte.Masquerade.Character;
using MischievousByte.Masquerade.Rig;
using MischievousByte.RigBuilder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LukewarmLobsters.OriginsUnknown
{
    public class CharacterControllerModule : RigModule
    {
        public SkeletonModule skeletonModule;

        private CharacterController characterController;

        public InputActionReference moveInput;
        public InputActionReference rotateInput;
        public float movementSpeed;
        public float rotationSpeed;

        public float linearGravity = -1;

        private void OnContextChanged(ModularRig.Context context)
        {
            characterController = Rig.GetComponent<CharacterController>();
            context.onUpdate.Bind(Execute, this);
        }


        private void Execute()
        {
            Rig.transform.rotation = Quaternion.Euler(Vector3.up * rotateInput.action.ReadValue<Vector2>().x * rotationSpeed * Time.deltaTime) * Rig.transform.rotation;

            Vector2 moveInput = this.moveInput.action.ReadValue<Vector2>();

            Vector3 move = skeletonModule.Velocity + new Vector3(moveInput.x, 0, moveInput.y) * movementSpeed;

            move = Rig.transform.rotation * move + Physics.gravity.normalized * linearGravity;

            characterController.Move(move * Time.deltaTime);
        }

        private void OnAvatarChanged(AvatarChangeContext context)
        {
            var cc = Rig.GetComponent<CharacterController>();

            cc.height = context.avatar.Height;

            float x = 0;
            x += context.avatar.shape.torso.hipsEllipse.XPositive;
            x += context.avatar.shape.torso.hipsEllipse.XNegative;
            x += context.avatar.shape.torso.waistEllipse.XPositive;
            x += context.avatar.shape.torso.waistEllipse.XNegative;
            x += context.avatar.shape.torso.chestEllipse.XPositive;
            x += context.avatar.shape.torso.chestEllipse.XNegative;
            x += context.avatar.shape.torso.sternumEllipse.XPositive;
            x += context.avatar.shape.torso.sternumEllipse.XNegative;

            float y = 0;
            y += context.avatar.shape.torso.hipsEllipse.YPositive;
            y += context.avatar.shape.torso.hipsEllipse.YNegative;
            y += context.avatar.shape.torso.waistEllipse.YPositive;
            y += context.avatar.shape.torso.waistEllipse.YNegative;
            y += context.avatar.shape.torso.chestEllipse.YPositive;
            y += context.avatar.shape.torso.chestEllipse.YNegative;
            y += context.avatar.shape.torso.sternumEllipse.YPositive;
            y += context.avatar.shape.torso.sternumEllipse.YNegative;

            float rT = Mathf.Lerp(x, y, 0.5f);

            rT /= 8f;

            cc.radius = rT;
            cc.center = Vector3.up * (cc.height / 2f);
        }
    }

}
