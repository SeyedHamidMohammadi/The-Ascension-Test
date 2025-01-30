using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Direction = PlayerMovement.Direction;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] private float health;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private Direction direction;
    [SerializeField] private bool running;
    public bool death;
    [SerializeField] private bool hit;
    public bool attack;
    [SerializeField] private bool inAttack;
    [SerializeField] private float distanceFromPlayer;
    [SerializeField] private Type type;
    public bool playerDetected;

    [Header("Options")]
    [SerializeField] private float speed = 3;
    [SerializeField] private float distanceToRecognition = 10;
    [SerializeField] private float distanceToAttack = 2;
    public float[] damage = new float[2]{5, 20};
    [SerializeField] private float walkAnimSpeed = 1.5f;
    
    [Header("Access")]
    public Rigidbody rigid;
    public Animator anim;
    public GameObject swordTrail;
    [SerializeField] private GameObject sword;
    [SerializeField] private GameObject rock;
    [SerializeField] private Transform startShootRock;

    [Header("UI")]
    [SerializeField] private GameObject healthBarPrefab;
    private Transform healthBar;
    private Image healthBarImage;
    
    [Header("VFX")]
    [SerializeField] private ParticleSystem[] hitVfx;
    
    //others
    private Vector3 velocity;
    private float velocity_X;
    private float force_X;
    private Coroutine attackRepeat;
    
    //animation layer weidtht
    private float movementLayerWeight;
    private float hitLayerWeight;
    private float combatLayerWeight;
    private float deathLayerWeight;
    
    private Transform canvas;
    private Router _router;

    public enum Type
    {
        Melee,
        Ranged
    }

    private void Awake()
    {
        _router = FindFirstObjectByType<Router>();
        canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
    }

    private void Start()
    {
        swordTrail.SetActive(false);
        rock.SetActive(false);
        
        sword.SetActive(type == Type.Melee);
        anim.SetBool("Melee", type == Type.Melee);
        
        SetupHealthBar();
        UpdateHealth(maxHealth);
    }

    void Update()
    {
        distanceFromPlayer = Vector3.Distance(transform.position, _router.player.movement.transform.position);
        
        SetVelocity();
        Act();
        SetUI();
        
        SetMovementAnim();
        SetHitAnim();
        SetCombatAnim();
        SetDeathAnim();
    }

    void SetupHealthBar()
    {
        healthBar = Instantiate(healthBarPrefab, canvas).transform;
        healthBarImage = healthBar.Find("bar").GetComponent<Image>();
    }

    void HideHealthBar()
    {
        healthBar.gameObject.SetActive(false);
    }
    
    void ShowHealthBar()
    {
        healthBar.gameObject.SetActive(true);
    }

    void Death()
    {
        anim.SetTrigger("DeathTrigger");
        death = true;
        HideHealthBar();
        StopMoving();
        _router.player.combat.UpdateCombatMode();
    }

    void Act()
    {
        if (death) return;
        
        if (distanceFromPlayer < distanceToRecognition)
        {
            if (distanceFromPlayer > distanceToAttack) // Reach to player
            {
                anim.SetFloat("speed", walkAnimSpeed);
                MoveToPlayer();

                if (inAttack)
                {
                    StopAttackRepeat();
                    inAttack = false;
                }
            }
            else // Reached to player
            {
                if (distanceFromPlayer < 0.7f) // Player is so close
                {
                    anim.SetFloat("speed", direction == Direction.Left ? -walkAnimSpeed : walkAnimSpeed);
                    Move(Direction.Right, false);
                    
                    if (inAttack)
                    {
                        StopAttackRepeat();
                        inAttack = false;
                    }
                }
                else
                {
                    anim.SetFloat("speed", walkAnimSpeed);
                    
                    if ((direction == Direction.Left &&
                         _router.player.movement.transform.position.x < transform.position.x) ||
                        (direction == Direction.Right &&
                         _router.player.movement.transform.position.x > transform.position.x))
                    {
                        StopMoving();

                        if (!inAttack)
                        {
                            StopAttackRepeat();
                            attackRepeat = StartCoroutine(AttackRepeat());
                            inAttack = true;
                        }
                    }
                    else
                    {
                        if (inAttack)
                        {
                            StopAttackRepeat();
                            inAttack = false;
                        }

                        MoveToPlayer();
                    }
                }
            }

            if (!playerDetected)
            {
                anim.SetBool("Combat", true);
                playerDetected = true;
                _router.player.combat.UpdateCombatMode();
            }
        }
        else if (playerDetected) // Idle
        {
            anim.SetBool("Combat", false);
            playerDetected = false;
            StopMoving();
            
            if (inAttack)
            {
                StopAttackRepeat();
                inAttack = false;
            }
        }
    }

    void StopAttackRepeat()
    {
        if (attackRepeat != null)
            StopCoroutine(attackRepeat);
    }

    void MoveToPlayer()
    {
        Direction dir = transform.position.x > _router.player.movement.transform.position.x
            ? Direction.Left
            : Direction.Right;
                
        Move(dir);
    }
    
    void StopMoving()
    {
        velocity = Vector3.zero;
        running = false;
        _router.player.combat.UpdateCombatMode();
    }
    
    void Move(Direction direction, bool forward = true)
    {
        this.direction = direction;
        int multiple = direction == Direction.Right ? 1 : -1;
        velocity = new Vector3(speed * multiple, velocity.y, velocity.z);
        running = true;

        if (forward)
        {
            float scaleAbs = Mathf.Abs(transform.localScale.z);
            SetScaleZ(direction == Direction.Right ? scaleAbs : -scaleAbs);
        }
    }
    
    void SetMovementAnim()
    {
        float lerpSpeed = 3;
         
        bool movement = running;
        movementLayerWeight = Mathf.Lerp(movementLayerWeight, movement ? 1 : 0, lerpSpeed * Time.deltaTime);
         
        int layerIndex = anim.GetLayerIndex("Movement");
        anim.SetLayerWeight(layerIndex, movementLayerWeight);
    }
    
    void SetDeathAnim()
    {
        float lerpSpeed = 3;
        
        deathLayerWeight = Mathf.Lerp(deathLayerWeight, death ? 1 : 0, lerpSpeed * Time.deltaTime);
         
        int layerIndex = anim.GetLayerIndex("Death");
        anim.SetLayerWeight(layerIndex, deathLayerWeight);
    }
    
    void SetHitAnim()
    {
        float lerpSpeed = 5;
        
        hitLayerWeight = Mathf.Lerp(hitLayerWeight, hit && !attack ? 1 : 0, lerpSpeed * Time.deltaTime);
        
        int layerIndex = anim.GetLayerIndex("Hit");
        anim.SetLayerWeight(layerIndex, hitLayerWeight);
    }
    
    void SetVelocity()
    {
        float lerpSpeed = 15;
        velocity_X = Mathf.Lerp(velocity_X, velocity.x + force_X, lerpSpeed * Time.deltaTime);
        rigid.velocity = new Vector3(velocity_X, rigid.velocity.y, rigid.velocity.z);
    }
    
    void SetScaleZ(float value)
    {
        Vector3 scale = transform.localScale;
        scale.z = value;
        transform.localScale = scale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Sword") && _router.player.combat.attack)
        {
            float damage = Random.Range(_router.player.combat.swordDamage[0], _router.player.combat.swordDamage[1]);
            TakeDamage((int)damage);
        }
    }

    IEnumerator AttackRepeat()
    {
        while (!death)
        {
            Attack();
            yield return new WaitForSeconds(type == Type.Melee ? 1 : 3);
        }
    }
    
    void Attack()
    {
        if (death) return;
         
        int attackId = Random.Range(1, 3);

        if (type == Type.Ranged)
            attackId = 3;
        
        anim.SetTrigger("AttackTrigger");
        anim.SetInteger("AttackId", attackId);
        anim.SetBool("Attack", true);
        attack = true;

        if (type == Type.Ranged)
        {
            StartCoroutine(ShootRock());
        }
        
        StopCoroutine(EndAttackDelay());
        StartCoroutine(EndAttackDelay());
    }

    IEnumerator ShootRock()
    {
        rock.SetActive(false);
        yield return new WaitForSeconds(0.35f);
        
        rock.transform.position = startShootRock.position;
        rock.SetActive(true);
        
        while (Vector3.Distance(rock.transform.position, _router.player.movement.transform.position) > 0.1f)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            Vector3 playerPos = _router.player.movement.transform.position;
            float rockDistanceToCenter = Vector3.Distance(rock.transform.position, GetMidPoint(transform.position, playerPos));
            int mutiple = transform.position.x > playerPos.x ? -1 : 1;
            rock.transform.position = Vector3.Lerp(rock.transform.position, _router.player.movement.transform.position + new Vector3(3 * mutiple, 3f / rockDistanceToCenter, 0), 5 * Time.deltaTime);
        }
        
        rock.SetActive(false);
    }
    
    Vector3 GetMidPoint(Vector3 point1, Vector3 point2)
    {
        return (point1 + point2) / 2f;
    }
    
    void SetCombatAnim()
    {
        float lerpSpeed = 3;
        combatLayerWeight = Mathf.Lerp(combatLayerWeight, attack ? 1 : 0, lerpSpeed * Time.deltaTime);
        
        int layerIndex = anim.GetLayerIndex("Combat");
        anim.SetLayerWeight(layerIndex, combatLayerWeight);
    }
    
    IEnumerator EndAttackDelay()
    {
        if (type == Type.Melee)
        {
            yield return new WaitForSeconds(0.1f);
            swordTrail.SetActive(true);
            yield return new WaitForSeconds(0.4f);
            swordTrail.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            attack = false;
        }
        else
        {
            yield return new WaitForSeconds(1);
            attack = false;
        }
    }

    void TakeDamage(int damage)
    {
        if (death) return;

        float newHealth = health - damage;
        UpdateHealth(newHealth > 0 ? newHealth : 0);

        if (newHealth <= 1f)
            Death();
        
        // force back
        StartCoroutine(ForceBack());

        // hit
        hit = true;
        PlayHitVfx();
        StartCoroutine(EndHit());
    }

    IEnumerator EndHit()
    {
        yield return new WaitForSeconds(0.1f);
        hit = false;
    }

    IEnumerator ForceBack()
    {
        int multiple = _router.player.movement.transform.position.x < transform.position.x ? 1 : -1;
        force_X = 1.5f * multiple;
        yield return new WaitForSeconds(0.3f);
        force_X = 0;
    }

    void SetUI()
    {
        healthBar.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 2.5f, 0));

        float scale = 17/Camera.main.fieldOfView;
        healthBar.localScale = new Vector3(scale, scale, scale);
    }

    void UpdateHealth(float value)
    {
        health = value;
        healthBarImage.fillAmount = health / maxHealth;
    }
    
    void PlayHitVfx()
    {
        int index = Random.Range(0, hitVfx.Length);
        hitVfx[index].Play();
    }
}
