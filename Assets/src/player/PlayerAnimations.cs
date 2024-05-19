using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    public Animator animator;
    private PlayerMovement playerMovement;
    private AnimatorOverrideController overrideController;

    void Start()
    {
        playerMovement = transform.GetComponent<PlayerMovement>();
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
    }

    private void Update()
    {
        animator.SetBool("IsGrounded", playerMovement.IsGrounded());
        animator.SetFloat("Speed", playerMovement.GetForwardVelocity());
    }

    public void StopCurrentAnimation()
    {
        animator.Play("EmptyState");
    }

    public void StopWalking()
    {
        animator.SetBool("IsWalking", false);
    }

    public void StartSprinting()
    {
        animator.SetBool("IsSprinting", true);
    }

    public void StopSprinting()
    {
        animator.SetBool("IsSprinting", false);
    }

    public void StartReload()
    {
        animator.SetTrigger("Reload");
    }

    public void StartAttack()
    {
        animator.SetTrigger("Attack");
    }

    public void StartAimedAttack()
    {
        animator.SetTrigger("AimedAttack");
    }

    public void AttackHit()
    {
        animator.SetTrigger("AttackHit");
    }

    public void TriggerJumpStart()
    {
        if(playerMovement.IsGrounded()) animator.SetTrigger("StartJump");
    }

    public void AdjustAnimationSpeed(float attackSpeed)
    {
        if (animator != null && attackSpeed > 0)
        {
            animator.speed = 1 / attackSpeed;
        }
    }

    public float CalculateClipLength(string clip)
    {
        float animationLength = GetClipLengthFromOverride(animator, clip);
        float currentAnimationSpeed = animator.speed;

        if (currentAnimationSpeed == 0)
            return 0;

        return animationLength / currentAnimationSpeed;
    }

    public float GetClipLengthFromOverride(Animator animator, string animationName)
    {
        AnimatorOverrideController overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
        if (overrideController == null)
        {
            Debug.LogError("Animator is not using an AnimatorOverrideController");
            return 0f;
        }

        foreach (var clipPair in overrideController.clips)
        {
            if (clipPair.originalClip.name == animationName)
            {
                return clipPair.overrideClip.length;
            }
        }

        Debug.LogError($"Animation clip not found in override controller: {animationName}");
        return 0f;
    }

    public float GetAnimationDuration(string animationName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }

        Debug.LogWarning($"Animation clip not found: {animationName}");
        return 0f;
    }

    public void SetItemAnimations(AnimatorOverrideController overrideController)
    {
        if (overrideController == null) return;
        animator.runtimeAnimatorController = overrideController;
    }

    public void ResetOverrides()
    {
        if (!overrideController) return;
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);

        overrideController.GetOverrides(overrides);

        for (int i = 0; i < overrides.Count; i++)
        {
            overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, null);
        }

        overrideController.ApplyOverrides(overrides);
        animator.runtimeAnimatorController = overrideController;
    }
}
