using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

//ts is crazy unused
public class SceneTransitionAnim : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] AnimationClip clip;

    void OnEnable()
    {
        if (animator == null || clip == null)
            return;

        AnimationPlayableUtilities.PlayClip(animator, clip, out _);
    }
}