using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//这个脚本来控制每个贴图的位置
//先从TileCamera中传入一个int，来告诉这个脚本，哪个0-255中哪个贴图被使用
//这个脚本是被TileCamera调用来使用的
public class Tile : MonoBehaviour
{
    [Header("Set Dynamically")]
    public int x;
    public int y;
    public int tileNum;
    private BoxCollider bColl;

    void Awake() {
        bColl = GetComponent<BoxCollider>();
    }

    // 从TileCamera中传入贴图的所在xy坐标位置和贴图的编号num
    // 目的是在全图中set哪个贴图和这个贴图的位置
    // 如果eTileNum为空，则会引用TileCamera.GET_MAP()
    public void SetTile(int eX, int eY, int eTileNum = -1) {
        x = eX;
        y = eY;
        //set贴图实例的位置
        transform.localPosition = new Vector3(x, y, 0);
        //set名字
        gameObject.name = x.ToString("D3") + "x" + y.ToString("D3");   // eg. x=23 y=5 => "023x005"
        if(eTileNum == -1) {
            //通过位置找到编号
            eTileNum = TileCamera.GET_MAP(x, y);
        } else {
            TileCamera.SET_MAP(x, y, eTileNum);    //此句用来开门锁时替换门的贴图，xy是门的位置，eTileNum是新替换的开门贴图的编号
        }
        tileNum = eTileNum;
        //set合适的贴图
        GetComponent<SpriteRenderer>().sprite = TileCamera.SPRITES[tileNum];
        SetCollider();
    }

    void SetCollider() {
        bColl.enabled = true;
        char c = TileCamera.COLLISIONS[tileNum];
        switch(c) {
            case 'S':  //全部碰撞
                bColl.center = Vector3.zero;
                bColl.size = Vector3.one;
                break;
            case 'W':   //上部碰撞
                bColl.center = new Vector3(0, 0.25f, 0);
                bColl.size = new Vector3(1, 0.5f, 1);
                break;
            case 'A':   //左部碰撞
                bColl.center = new Vector3(-0.25f, 0, 0);
                bColl.size = new Vector3(0.5f, 1, 1);
                break;
            case 'D':   //右部碰撞
                bColl.center = new Vector3(0.25f, 0, 0);
                bColl.size = new Vector3(0.5f, 1, 1);
                break;
            //下面是四个角落的碰撞，可选
            case 'Q': // Top, Left
                bColl.center = new Vector3( -0.25f, 0.25f, 0 );
                bColl.size = new Vector3( 0.5f, 0.5f, 1 );
                break;
            case 'E': // Top, Right
                bColl.center = new Vector3( 0.25f, 0.25f, 0 );
                bColl.size = new Vector3( 0.5f, 0.5f, 1 );
                break;
            case 'Z': // Bottom, left
                bColl.center = new Vector3( -0.25f, -0.25f, 0 );
                bColl.size = new Vector3( 0.5f, 0.5f, 1 );
                break;
            case 'X': // Bottom
                bColl.center = new Vector3( 0, -0.25f, 0 );
                bColl.size = new Vector3( 1, 0.5f, 1 );
                break;
            case 'C': // Bottom, Right
                bColl.center = new Vector3( 0.25f, -0.25f, 0 );
                bColl.size = new Vector3( 0.5f, 0.5f, 1 );
                break;
            default:
                bColl.enabled = false;
                break;    
        }
    }
}
