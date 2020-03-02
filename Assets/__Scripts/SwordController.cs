using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    private GameObject sword;
    private Dray dray;

    // Start is called before the first frame update
    void Start()
    {
        sword = transform.Find("Sword").gameObject;    //找到当前controller的子对象Sword
        dray = transform.parent.GetComponent<Dray>();    //找到当前controller的父对象的Dray类的实例
        sword.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0,0,90*dray.facing);
        sword.SetActive(dray.mode == Dray.eMode.attack);
    }
}
