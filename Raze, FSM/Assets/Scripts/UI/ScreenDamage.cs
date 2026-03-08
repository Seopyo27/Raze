using System;
using System.Collections;
using MP1.Stats;
using Unity.VisualScripting;
using UnityEngine;

namespace MP1.UI
{
    public class ScreenDamage : MonoBehaviour {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] float fadeInTime = 0.3f;
        [SerializeField] float fadeOutTime = 3f;
        Coroutine currentActiveFade = null;


        private void Awake()
        {
            canvasGroup.alpha = 0;
        }

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Health health = player.GetComponent<Health>();
                if (health != null)
                {
                    health.OnHit += FadeInOut;
                    health.OnDie += FadeIn;
                }
            }
        }

        public void FadeIn()
        {
            if (currentActiveFade != null)
            {
                StopCoroutine(currentActiveFade);
            }

            currentActiveFade = StartCoroutine(FadeRoutine(1, fadeInTime));
        }

        public void FadeInOut()
        {
            if (currentActiveFade != null)
            {
                StopCoroutine(currentActiveFade);
            }
            currentActiveFade = StartCoroutine(FadeInOutRoutine());
        }

        private IEnumerator FadeInOutRoutine()
        {
            yield return FadeRoutine(1, fadeInTime);
            yield return FadeRoutine(0, fadeOutTime);
        }

        private IEnumerator FadeRoutine(float target, float time)
        {

            while (!Mathf.Approximately(canvasGroup.alpha, target))
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, Time.deltaTime / time);
                yield return null;
            }
        }
        
    }
}