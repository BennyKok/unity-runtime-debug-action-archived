using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BennyKok.RuntimeDebug.Demo.DashsuCube
{
    public class BouncyScaleUp : MonoBehaviour
    {
        public float duration = 0.5f;
        public AnimationCurve curve;

        private Vector3 originalScale;
        private bool animating;
        private float startTime;


        private void Awake()
        {
            originalScale = transform.localScale;
        }

        private void Start()
        {
            transform.localScale = Vector3.zero;
            animating = true;
            startTime = Time.time;
        }

        private void Update()
        {
            if (animating)
            {
                var animeTime = (Time.time - startTime) / duration;
                var newScale = originalScale * curve.Evaluate(animeTime);
                transform.localScale = newScale;

                if (animeTime >= 1)
                    animating = false;
            }
        }
    }
}