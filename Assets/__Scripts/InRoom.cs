using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRoom : MonoBehaviour
{
    public static float ROOM_W = 16;
    public static float ROOM_H = 11;
    public static float WALL_T = 2;
    // the maximum boundary of this map, change it when changing the map
    public static int MAX_RM_X = 9;
    public static int MAX_RM_Y = 9;
    public static Vector2[] DOORS = new Vector2[] {
        new Vector2(14, 5),
        new Vector2(7.5f, 9),
        new Vector2(1, 5),
        new Vector2(7.5f, 1)
    };

    [Header("Set in Inspector")]
    public bool keepInRoom = true;
    public float gridMult = 1;

    void LateUpdate() {
        //在Update后，确保当前角色在room内，而不移动出去
        if(keepInRoom) {
            Vector2 rPos = roomPos;
            rPos.x = Mathf.Clamp(rPos.x, WALL_T, ROOM_W - 1 -WALL_T);
            rPos.y = Mathf.Clamp(rPos.y, WALL_T, ROOM_H - 1 -WALL_T);
            roomPos = rPos;
        }
    }

    //角色目前在当前房间的local坐标
    public Vector2 roomPos {
        get {
            Vector2 tPos = transform.position;
            tPos.x %= ROOM_W;
            tPos.y %= ROOM_H;
            return tPos;    //角色当前与房间的相对位置
        }
        set {
            Vector2 rm = roomNum;
            rm.x *= ROOM_W;
            rm.y *= ROOM_H;
            rm += value;    //value传入的是相对坐标
            transform.position = rm;
        }
    }

    //当前角色在哪个房间？
    public Vector2 roomNum {
        get {
            Vector2 tPos = transform.position;
            tPos.x = Mathf.Floor(tPos.x / ROOM_W);
            tPos.y = Mathf.Floor(tPos.y / ROOM_H);
            return tPos;    //当前房间左下角的x y坐标
        }
        //下面是根据roomNum来设置当前角色的全局坐标
        set {
            Vector2 rPos = roomPos;    //在当前角色与房间的相对坐标
            Vector2 rm = value;
            rm.x *= ROOM_W;        //当前room相对与全图的坐标
            rm.y *= ROOM_H;
            transform.position = rm + rPos;
        }
    }

    //找到距离角色最近的网格距离
    public Vector2 GetRoomPosOnGrid(float mult = -1) {
        if(mult == -1) {
            mult = gridMult;    // 1m
        }
        Vector2 rPos = roomPos;
        rPos /= mult;
        rPos.x = Mathf.Round(rPos.x);
        rPos.y = Mathf.Round(rPos.y);
        rPos *= mult;
        return rPos;
    }
}
