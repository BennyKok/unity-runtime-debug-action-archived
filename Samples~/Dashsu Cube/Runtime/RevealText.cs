using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BennyKok.RuntimeDebug.Demo.DashsuCube
{
    /// <summary>
    /// Simple script to trigger reveal animation in the Animator
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class RevealText : MonoBehaviour
    {
        [System.NonSerialized] public Animator animator;
        [System.NonSerialized] public TextMeshProUGUI text;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            text = GetComponent<TextMeshProUGUI>();
        }

        public void Reveal()
        {
            animator.SetTrigger("Reveal");
        }
    }
}