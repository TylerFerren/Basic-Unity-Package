
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codesign
{
    public class AnimationParameters : MonoBehaviour
    {
        [SerializeField] private Animator anim;

        private Coroutine aimingCoroutine;
        public void OnMove(Vector3 velocity)
        {
            var locVel = transform.InverseTransformDirection(velocity);

            anim.SetFloat("MovementMag", new Vector3(locVel.x, 0, locVel.z).magnitude);

            anim.SetFloat("MovementX", locVel.x);
            anim.SetFloat("MovementY", locVel.y);
            anim.SetFloat("MovementZ", locVel.z);
        }

        public void OnJump()
        {
            anim.SetTrigger("Jump");
        }

        public void OnGrouned(bool isGrouned)
        {
            anim.SetBool("Grounded", isGrouned);
        }

        public void OnTimeToLand(float time)
        {
            anim.SetFloat("LandTime", time);
        }

        public void OnAim(bool aim)
        {
            anim.SetBool("Aiming", aim);
            if (aimingCoroutine != null) StopCoroutine(aimingCoroutine);
            if (aim) aimingCoroutine = StartCoroutine(LerpValue(anim.GetLayerWeight(1), 1, 1));
            else aimingCoroutine = StartCoroutine(LerpValue(anim.GetLayerWeight(1), 0, 1));
        }

        public IEnumerator LerpValue(float current, float target, float seconds)
        {
            while (Mathf.Abs(target - current) > 0.05f)
            {
                current = Mathf.MoveTowards(current, target, seconds * Time.fixedDeltaTime);
                anim.SetLayerWeight(1, current);
                yield return new WaitForFixedUpdate();
            }
        }
    }

}