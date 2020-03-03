using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 此脚本是用来跟踪敌人对角色造成的伤害；已经攻击会不会造成角色的短位移；
// 先attach到敌人上；之后会attach到角色的武器上

public class DamageEffect: MonoBehaviour {
    [Header("Set in Inspector")]
    public int damage = 1;
    public bool knockback = true;
}
