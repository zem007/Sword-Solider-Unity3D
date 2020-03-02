using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateKeeper : MonoBehaviour
{
    // 关闭门的贴图编号
    const int lockedR = 95;
    const int lockedUR = 81;
    const int lockedUL = 80;
    const int lockedL = 100;
    const int lockedDL = 101;
    const int lockedDR = 102;

    // 开启的门的贴图编号
    const int openR = 48;
    const int openUR = 93;
    const int openUL = 92;
    const int openL = 51;
    const int openDL = 26;
    const int openDR = 27;

    private IKeyMaster keys;  //定位到此GameObject的IKeyMaster接口

    void Awake() {
        keys = GetComponent<IKeyMaster>();
    }

    void OnCollisionStay(Collision coll) {
        if(keys.keyCount < 1) return;

        //coll是Tile的一个实例，此处获取实例组件Tile
        Tile ti = coll.gameObject.GetComponent<Tile>();  
        if(ti == null) return;

        //只有在facing一致时才能开门
        int facing = keys.GetFacing();

        //检查是不是个door Tile
        Tile ti2;
        switch(ti.tileNum) {
            case lockedR:
                if(facing != 0) return;
                ti.SetTile(ti.x, ti.y, openR);
                break;
            case lockedUR:
                if(facing != 1) return;
                ti.SetTile(ti.x, ti.y, openUR);
                ti2 = TileCamera.TILES[ti.x - 1, ti.y];
                ti2.SetTile(ti2.x, ti2.y, openUL);
                break;
            case lockedUL:
                if(facing != 1) return;
                ti.SetTile(ti.x, ti.y, openUL);
                ti2 = TileCamera.TILES[ti.x + 1, ti.y];
                ti2.SetTile(ti2.x, ti2.y, openUR);
                break;
            case lockedL:
                if(facing != 2) return;
                ti.SetTile(ti.x, ti.y, openL);
                break;
            case lockedDR:
                if(facing != 3) return;
                ti.SetTile(ti.x, ti.y, openDR);
                ti2 = TileCamera.TILES[ti.x - 1, ti.y];
                ti2.SetTile(ti2.x, ti2.y, openDL);
                break;
            case lockedDL:
                if(facing != 3) return;
                ti.SetTile(ti.x, ti.y, openDL);
                ti2 = TileCamera.TILES[ti.x + 1, ti.y];
                ti2.SetTile(ti2.x, ti2.y, openDR);
                break;
            default:
                return;
        }
        keys.keyCount--;
    }
}
