using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Set in Inspector: Enemy")]
    public float maxHealth = 1;
    public float knockbackSpeed = 10;
    public float knockbackDuration = 0.25f;
    public float invincibleDuration = 0.5f;
    public GameObject[] randomItemDrops;
    public GameObject guaranteedItemDrop = null;

    [Header("Set Dynamically: Enemy")]
    public float health;
    public bool invincible = false;
    public bool knockback = false;

    private float knockbackDone = 0;
    private float invincibleDone = 0;
    private Vector3 knockbackVel;

    protected Animator anim;
    protected Rigidbody rigid;
    protected SpriteRenderer sRend;

    protected static Vector3[] directions = new Vector3[] { 
        Vector3.right, Vector3.up, Vector3.left, Vector3.down
    };

    protected virtual void Awake() 
    {
        health = maxHealth;
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        sRend = GetComponent<SpriteRenderer>();
    }  

    protected virtual void Update() {
        //检查是否无敌状态
        if(invincible && Time.time > invincibleDone) invincible = false;
        sRend.color = invincible ? Color.red: Color.white;
        //检查是否被击退
        if(knockback) {
            rigid.velocity = knockbackVel;
            if(Time.time <= knockbackDone) return;
        }

        anim.speed = 1; 
        knockback = false;
    }

    void OnTriggerEnter(Collider colld) {   //Enemy对象碰触到带有Trigger的Collision， 比如角色的剑
        if(invincible) return;
        DamageEffect dEf = colld.gameObject.GetComponent<DamageEffect>();
        if(dEf == null) return;

        health -= dEf.damage;
        if(health <= 0) Die();

        invincible = true;
        invincibleDone = Time.time + invincibleDuration;

        if(dEf.knockback) {
            Vector3 delta = transform.position - colld.transform.root.position;    //root.position 是指剑的根对象，角色
            if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) {
                //水平方向
                delta.x = (delta.x > 0) ? 1:-1;
                delta.y = 0;
            } else {
                //垂直方向
                delta.x = 0;
                delta.y = (delta.y > 0) ? 1:-1;
            }

            //设置Enemy的速度
            knockbackVel = delta * knockbackSpeed;
            rigid.velocity = knockbackVel;

            knockback = true;
            knockbackDone = Time.time + knockbackDuration;
            anim.speed = 0;
        }
    }

    void Die() {
        GameObject go;
        if(guaranteedItemDrop != null) {
            go = Instantiate<GameObject>(guaranteedItemDrop);
            go.transform.position = transform.position;
        } else if(randomItemDrops.Length > 0) {
            int n = Random.Range(0, randomItemDrops.Length);
            GameObject prefab = randomItemDrops[n];
            if(prefab != null) {
                go = Instantiate<GameObject>(prefab);
                go.transform.position = transform.position;
            }
        }
        Destroy(gameObject);
    }
}
