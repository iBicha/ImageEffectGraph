using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImageEffectGraph.Demo
{
    public class TransitionController : MonoBehaviour
    {
        public Material material;
        public float speed = 1f;
    
        public void StartTransition()
        {
            StopAllCoroutines();
            var startPosition = material.GetFloat("_Progress");
            var direction = startPosition > 0.5 ? -1f : 1f;
            StartCoroutine(TransitionRoutine(startPosition, direction));
        }


        IEnumerator TransitionRoutine(float startPosition, float direction)
        {
            while (direction > 0 && !Mathf.Approximately(startPosition, 1) ||
                   direction < 0 && !Mathf.Approximately(startPosition, 0))
            {
                startPosition = Mathf.Clamp01(startPosition + Time.deltaTime * direction * speed);
                material.SetFloat("_Progress", startPosition);
                yield return null;
            }
        }
    }
}
