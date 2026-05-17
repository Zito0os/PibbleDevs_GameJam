using UnityEngine;

public class SpellProjectile : MonoBehaviour
{
    public enum SpellEffectType
    {
        Slow,
        Freeze,
        Clear
    }

    [Header("Effect")]
    public SpellEffectType effectType = SpellEffectType.Slow;
    public float slowMultiplier = 0.6f;
    public float slowDuration = 7f;
    public float freezeDuration = 3f;

    private Transform owner;

    public void SetOwner(Transform ownerTransform)
    {
        owner = ownerTransform;
    }

    public void ConfigureFromItem(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.SpellSlow:
                effectType = SpellEffectType.Slow;
                break;
            case ItemType.SpellFreeze:
                effectType = SpellEffectType.Freeze;
                break;
            case ItemType.SpellClear:
                effectType = SpellEffectType.Clear;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other.transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.transform);
    }

    private void HandleHit(Transform hitTransform)
    {
        if (owner != null && hitTransform.root == owner.root)
            return;

        PlayerMovement target = hitTransform.GetComponentInParent<PlayerMovement>();
        if (target == null)
            return;

        switch (effectType)
        {
            case SpellEffectType.Slow:
                SoundManager.PlaySound(SoundType.SlowLlegado);
                target.ApplySlow(slowMultiplier, slowDuration);
                break;
            case SpellEffectType.Freeze:
                SoundManager.PlaySound(SoundType.FreezeLlegado);
                target.ApplyFreeze(freezeDuration);
                break;
            case SpellEffectType.Clear:
                SoundManager.PlaySound(SoundType.ClearLlegado);
                target.ClearInventoryFromSpell();
                break;
        }

        Destroy(gameObject);
    }
}
