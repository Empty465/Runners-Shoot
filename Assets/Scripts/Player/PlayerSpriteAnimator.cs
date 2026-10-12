using System;
using UnityEngine;

// Drives the player's sprite frames from gameplay state. No Animator controller is needed.
public class PlayerSpriteAnimator : MonoBehaviour
{
    [Serializable]
    public class Clip
    {
        public string name;
        public Sprite[] frames;
        public float fps = 10f;
        public bool loop = true;
    }

    [SerializeField] private Clip[] clips;
    [SerializeField] private int sortingOrder = 20;
    [SerializeField] private float hitDuration = 0.25f;
    [SerializeField] private float shootPoseDuration = 0.25f;

    private SpriteRenderer spriteRenderer;
    private PlayerController controller;
    private PlayerHealth health;
    private PlayerDodge dodge;
    private PlayerParry parry;
    private PlayerWeapon weapon;
    private PlayerMelee melee;
    private Rigidbody2D body;

    private string currentClip;
    private float clipStartTime;
    private string oneShotClip;
    private float oneShotUntil;
    private float shootUntil;
    private int facing = 1;
    private bool dead;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        health = GetComponent<PlayerHealth>();
        dodge = GetComponent<PlayerDodge>();
        parry = GetComponent<PlayerParry>();
        weapon = GetComponent<PlayerWeapon>();
        melee = GetComponent<PlayerMelee>();
        body = GetComponent<Rigidbody2D>();
        CreateVisual();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.DamageTaken += OnDamageTaken;
            health.Died += OnDied;
        }

        if (weapon != null)
        {
            weapon.Fired += OnFired;
        }

        if (melee != null)
        {
            melee.AttackPerformed += OnAttackPerformed;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.DamageTaken -= OnDamageTaken;
            health.Died -= OnDied;
        }

        if (weapon != null)
        {
            weapon.Fired -= OnFired;
        }

        if (melee != null)
        {
            melee.AttackPerformed -= OnAttackPerformed;
        }
    }

    private void CreateVisual()
    {
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(transform, false);

        float feetY = -0.9f;
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            feetY = box.offset.y - box.size.y * 0.5f;
        }

        visual.transform.localPosition = new Vector3(0f, feetY, 0f);
        spriteRenderer = visual.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = sortingOrder;
    }

    private void OnDamageTaken(bool armorOnly)
    {
        if (dead)
        {
            return;
        }

        StartOneShot("hit", hitDuration);
    }

    private void OnDied()
    {
        dead = true;
        Play("death");
    }

    private void OnFired()
    {
        shootUntil = Time.time + shootPoseDuration;
    }

    private void OnAttackPerformed(MeleeAttackKind kind)
    {
        switch (kind)
        {
            case MeleeAttackKind.Heavy:
                StartOneShot("heavy", ClipDuration("heavy"));
                break;
            case MeleeAttackKind.Execute:
                StartOneShot("execute", ClipDuration("execute"));
                break;
            default:
                StartOneShot("slash", ClipDuration("slash"));
                break;
        }
    }

    private void StartOneShot(string clipName, float duration)
    {
        oneShotClip = clipName;
        oneShotUntil = Time.time + duration;
        Play(clipName, true);
    }

    private void Update()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        UpdateFacing();

        if (dead)
        {
            Play("death");
            Render();
            return;
        }

        Play(ChooseClip());
        Render();
    }

    private void UpdateFacing()
    {
        if (melee != null)
        {
            facing = melee.FacingDirection;
        }
        else if (body != null && Mathf.Abs(body.linearVelocity.x) > 0.1f)
        {
            facing = body.linearVelocity.x > 0f ? 1 : -1;
        }

        spriteRenderer.flipX = facing < 0;
    }

    private string ChooseClip()
    {
        if (Time.time < oneShotUntil)
        {
            return oneShotClip;
        }

        if (dodge != null && dodge.IsDodging)
        {
            return "dash";
        }

        if (parry != null && parry.IsParryActive)
        {
            return "parry";
        }

        if (melee != null && melee.IsHolding)
        {
            return "charge";
        }

        if (Time.time < shootUntil)
        {
            return "shoot";
        }

        bool grounded = controller == null || controller.IsGrounded();
        if (!grounded)
        {
            return "jump";
        }

        return body != null && Mathf.Abs(body.linearVelocity.x) > 0.2f ? "run" : "idle";
    }

    private void Play(string clipName, bool restart = false)
    {
        if (restart || clipName != currentClip)
        {
            currentClip = clipName;
            clipStartTime = Time.time;
        }
    }

    private void Render()
    {
        Clip clip = FindClip(currentClip);
        if (clip == null || clip.frames == null || clip.frames.Length == 0)
        {
            return;
        }

        int index;
        if (clip.name == "jump")
        {
            float vy = body != null ? body.linearVelocity.y : 0f;
            index = vy > 1f ? 0 : (vy < -1f ? Mathf.Min(2, clip.frames.Length - 1) : Mathf.Min(1, clip.frames.Length - 1));
        }
        else
        {
            int step = Mathf.FloorToInt((Time.time - clipStartTime) * Mathf.Max(1f, clip.fps));
            index = clip.loop ? step % clip.frames.Length : Mathf.Min(step, clip.frames.Length - 1);
        }

        spriteRenderer.sprite = clip.frames[index];
    }

    private float ClipDuration(string clipName)
    {
        Clip clip = FindClip(clipName);
        if (clip == null || clip.frames == null || clip.frames.Length == 0)
        {
            return 0.3f;
        }

        return clip.frames.Length / Mathf.Max(1f, clip.fps);
    }

    private Clip FindClip(string clipName)
    {
        if (clips == null)
        {
            return null;
        }

        foreach (Clip clip in clips)
        {
            if (clip.name == clipName)
            {
                return clip;
            }
        }

        return null;
    }
}
