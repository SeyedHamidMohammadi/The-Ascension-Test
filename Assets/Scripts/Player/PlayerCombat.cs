using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerCombat : MonoBehaviour
{
    [Header("Status")]
    public bool attack;
    public bool mode;
    [SerializeField] private bool hit;
    public bool death;

    [Header("Options")]
    public float[] swordDamage = new float[2]{1, 1};
    public float[] rockDamage = new float[2]{1, 1};
    
    [Header("Access")]
    [SerializeField] private Enemy[] _enemies;
    [SerializeField] private GameObject swordTrail;
    [SerializeField] private Transform rightHandBone;
    [SerializeField] private Transform spine2Bone;
    [SerializeField] private Transform sword;
    [SerializeField] private Transform[] swordPos;
    
    [Header("VFX")]
    [SerializeField] private ParticleSystem[] hitVfx;

    [Header("UI")]
    public GameObject pauseUI;
    
    //others
    private float combatLayerWeight;
    private float hitLayerWeight;
    private float deathLayerWeight;
    public float force_X;

    private Router _router;
    
    private void Start()
    {
        _router = FindFirstObjectByType<Router>();
        
        UpdateEnemies();
        swordTrail.SetActive(false);
        UpdateCombatMode();
        
        pauseUI.SetActive(false);
    }

    void Update()
    {
        Controller();
        
        SetCombatAnim();
        SetHitAnim();
        SetDeathAnim();
        
        if (death)
            _router.player.status.rigid.velocity = Vector3.zero;
    }

    void Controller()
    {
        if (death) return;
        
        if (Input.GetMouseButtonDown(0) && !attack && mode)
            Attack();
    }

    void Attack()
    {
        int attackId = Random.Range(-3, 3);
        
        _router.player.status.anim.SetTrigger("AttackTrigger");
        _router.player.status.anim.SetInteger("AttackId", Mathf.Abs(attackId));
        _router.player.status.anim.SetBool("Attack", true);
        attack = true;
        
        StopCoroutine(EndAttackDelay());
        StartCoroutine(EndAttackDelay());

        int slowMotion = Random.Range(0, 20);
        
        if (slowMotion == 8)
            StartCoroutine(_router.main.gameController.SlowMotion(1));
    }

    void SetCombatAnim()
    {
        float lerpSpeed = 3;
        bool combat = attack && !_router.player.climb.climbing;
        combatLayerWeight = Mathf.Lerp(combatLayerWeight, combat ? 1 : 0, lerpSpeed * Time.deltaTime);
        
        int layerIndex = _router.player.status.anim.GetLayerIndex("Combat");
        _router.player.status.anim.SetLayerWeight(layerIndex, combatLayerWeight);
    }
    
    void SetDeathAnim()
    {
        float lerpSpeed = 3;
        deathLayerWeight = Mathf.Lerp(deathLayerWeight, death ? 1 : 0, lerpSpeed * Time.deltaTime);
        
        int layerIndex = _router.player.status.anim.GetLayerIndex("Death");
        _router.player.status.anim.SetLayerWeight(layerIndex, deathLayerWeight);
    }

    void Death()
    {
        _router.player.status.anim.SetTrigger("DeathTrigger");
        death = true;
        pauseUI.SetActive(true);
    }
    
    void SetHitAnim()
    {
        float lerpSpeed = 5;
        
        hitLayerWeight = Mathf.Lerp(hitLayerWeight, hit && !attack ? 1 : 0, lerpSpeed * Time.deltaTime);
        
        int layerIndex = _router.player.status.anim.GetLayerIndex("Hit");
        _router.player.status.anim.SetLayerWeight(layerIndex, hitLayerWeight);
    }

    IEnumerator EndAttackDelay()
    {
        yield return new WaitForSeconds(0.1f);
        swordTrail.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        swordTrail.SetActive(false);
        yield return new WaitForSeconds(0.05f);
        attack = false;
    }

    public void UpdateCombatMode()
    {
        mode = CheckCombat();
        sword.SetParent(mode ? rightHandBone : spine2Bone);
        StartCoroutine(UpdateSwordTransform(mode ? swordPos[1] : swordPos[0]));
        StartCoroutine(_router.camera.zoom.UpdateZoom());
        _router.player.status.anim.SetBool("Combat", mode);
    }

    bool CheckCombat()
    {
        foreach (Enemy enemy in _enemies)
        {
            if (enemy.playerDetected && !enemy.death)
                return true;
        }

        return false;
    }

    void UpdateEnemies()
    {
        _enemies = FindObjectsOfType<Enemy>();
    }
    
    public void TakeDamage(int damage, Enemy enemy = null)
    {
        if (death) return;
        
        float newHealth = _router.player.status.health - damage;
        _router.player.status.UpdateHealth(newHealth < 0 ? 0 : newHealth);
        
        if (newHealth <= 0)
            Death();
        
        StartCoroutine(EndHit());
        
        if (enemy != null)
            StartCoroutine(ForceBack(enemy));

        hit = true;
        PlayHitVfx();
    }
    
    IEnumerator EndHit()
    {
        yield return new WaitForSeconds(0.1f);
        hit = false;
    }
    
    IEnumerator ForceBack(Enemy enemy)
    {
        int multiple = transform.position.x < enemy.transform.position.x ? -1 : 1;
        force_X = 1 * multiple;
        yield return new WaitForSeconds(0.15f);
        force_X = 0;
    }

    IEnumerator UpdateSwordTransform(Transform target)
    {
        float lerpSpeed = 15f;
        
        while (Vector3.Distance(sword.position, target.position) > 0.1f || Quaternion.Angle(sword.rotation, target.rotation) > 1f)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            sword.position = Vector3.Lerp(sword.position, target.position, lerpSpeed * Time.deltaTime);
            sword.rotation = Quaternion.Lerp(sword.rotation, target.rotation, lerpSpeed * Time.deltaTime);
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Rock"))
        {
            float damage = Random.Range(rockDamage[0], rockDamage[1]);
            TakeDamage((int)damage);
            other.gameObject.SetActive(false);
        }
    }
    
    void PlayHitVfx()
    {
        int index = Random.Range(0, hitVfx.Length);
        hitVfx[index].Play();
    }
}
