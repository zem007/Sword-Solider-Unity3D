using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollowDray : MonoBehaviour
{
    public static bool TRANSITIONING = false;
    [Header("Set in Inspector")]
    public InRoom drayInRm;
    public float transTime = 0.5f;

    private Vector3 p0, p1;
    private InRoom inRm;
    private float transStart;

    void Awake() {
        inRm = GetComponent<InRoom>();
    }

    void Update() {
        if(TRANSITIONING) {
            float u = (Time.time - transStart) / transTime;
            if(u >= 1) {
                u = 1;
                TRANSITIONING = false;
            }
            transform.position = (1 - u)* p0 + u * p1;
        } else {
            if(drayInRm.roomNum != inRm.roomNum) {    // 摄像机的roomNum和角色的roomNum不在同一个房间时才触发
                TransitionTo(drayInRm.roomNum);
            }
        }
    }

    void TransitionTo(Vector2 rm) {
        p0 = transform.position;
        inRm.roomNum = rm;    //此处设置摄像机在新的room中的全局坐标，但是是Vector2，需要通过下一句转化为Vector3
        p1 = transform.position + (Vector3.back * 10);
        transform.position = p0;

        transStart = Time.time;
        TRANSITIONING = true;
    }
}
