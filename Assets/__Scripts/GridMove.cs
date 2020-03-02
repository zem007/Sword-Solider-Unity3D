using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMove : MonoBehaviour
{
    private IFacingMover mover;

    void Awake() {
        mover = GetComponent<IFacingMover>();    //找到此GamObject的使用过IFacingMover接口的组件，此处是Dray组件脚本
    }

    void FixedUpdate() {
        if(!mover.moving) return;
        int facing = mover.GetFacing();

        //先得到grid位置
        Vector2 rPos = mover.roomPos;
        Vector2 rPosGrid = mover.GetRoomPosOnGrid();

        //移动到grid line, 与grid平齐
        float delta = 0;
        if(facing == 0 || facing == 2) {
            delta = rPosGrid.y - rPos.y;   //垂直方向上与y grid计算差值
        } else {
            delta = rPosGrid.x - rPos.x;   //水平方向上
        }
        if(delta == 0) return;    //已经平齐了，不需要额外移动

        float move = mover.GetSpeed() * Time.fixedDeltaTime;    //如果是锁帧60帧，fixedDeltaTime恒等于1/60s
        move = Mathf.Min(move, Mathf.Abs(delta));
        if(delta < 0) move = -move;
        if(facing == 0 || facing == 2) {
            rPos.y += move;
        } else {
            rPos.x += move;
        }

        mover.roomPos = rPos;
    }
}
