using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private Animator animator;

    // Hashes de parámetros
    private readonly int isRunningParam = Animator.StringToHash("IsRunning");
    private readonly int fireAttackParam = Animator.StringToHash("F_Attack");
    private readonly int waterShieldParam = Animator.StringToHash("W_Shield");

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // ---- Métodos Públicos ----
    public void SetRunning(bool state)
    {
        animator.SetBool(isRunningParam, state);
    }

    public void PlayFireAttack()
    {
        animator.SetTrigger(fireAttackParam);
    }

    public void PlayWaterShield()
    {
        animator.SetTrigger(waterShieldParam);
    }
}
