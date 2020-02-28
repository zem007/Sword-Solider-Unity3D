using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dray : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float speed = 5f;
    [Header("Set Dynamically")]
    public int dirHeld = -1;    //按住的方向键所指向的方向

    private Rigidbody rigid;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        dirHeld = -1;
        if(Input.GetKey(KeyCode.RightArrow)) dirHeld = 0;
        if(Input.GetKey(KeyCode.UpArrow)) dirHeld = 1;
        if(Input.GetKey(KeyCode.LeftArrow)) dirHeld = 2;
        if(Input.GetKey(KeyCode.DownArrow)) dirHeld = 3;

        Vector3 vel = Vector3.zero;
        switch(dirHeld) {
            case 0:
                vel = Vector3.right;
                break;
            case 1:
                vel = Vector3.up;
                break;
            case 2:
                vel = Vector3.left;
                break;
            case 3:
                vel = Vector3.down;
                break;
        }
        rigid.velocity = vel * speed;
    }
}
