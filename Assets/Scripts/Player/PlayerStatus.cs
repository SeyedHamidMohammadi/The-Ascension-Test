using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    [Header("Status")]
    public float health;
    [SerializeField] private float maxHealth = 100;

    [Header("UI")]
    [SerializeField] private Transform healthBarTrans;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Transform arrow;
    
    [Header("Access")]
    public Rigidbody rigid;
    public Animator anim;
    public CapsuleCollider collider;
    
    void Awake()
    {
        UpdateHealth(100);
    }

    void Update()
    {
        SetHealthBar();
        SetArrow();
    }

    void SetHealthBar()
    {
        healthBarTrans.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 2.2f, 0));

        float scale = 17/Camera.main.fieldOfView;
        healthBarTrans.localScale = new Vector3(scale, scale, scale);
    }

    void SetArrow()
    {
        arrow.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 1.9f, 0));
        
        float scale = 10/Camera.main.fieldOfView;
        arrow.localScale = new Vector3(scale, scale, scale);
    }

    public void UpdateHealth(float value)
    {
        health = value;
        healthBarImage.fillAmount = health / maxHealth;
    }
}
