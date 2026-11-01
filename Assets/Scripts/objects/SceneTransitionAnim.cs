using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

//ts is crazy
public class SceneTransitionAnim : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] AnimationClip clip;

    void OnEnable()
    {
        AnimationPlayableUtilities.PlayClip(animator, clip, out _);
    }
}