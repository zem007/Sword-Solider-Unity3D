using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    public enum eMode { none, gOut, gInMiss, gInHit }

    [Header("Set in Inspector")]
    public float grappleSpd = 10;
    public float grappleLength = 7;
    public float grappleInLength = 0.5f;
    public int unsafeTileHealthPenalty = 2;
    public TextAsset mapGrappleable;

    [Header("Set Dynamically")]
    public eMode mode = eMode.none;
    //可以被勾到的Tile
    public List<int> grappleTiles;
    public List<int> unsafeTiles;

    private Dray dray;
    private Rigidbody rigid;
    private Animator anim;
    private Collider drayColld;

    private GameObject grapHead;
    private LineRenderer grapLine;

    private Vector3 p0, p1;
    private int facing;

    private Vector3[] directions = new Vector3[] {
        Vector3.right, Vector3.up, Vector3.left, Vector3.down
    };

    void Awake() {
        string gTiles = mapGrappleable.text;
        gTiles = Utils.RemoveLineEndings(gTiles);
        grappleTiles = new List<int>();
        unsafeTiles = new List<int>();

        //根据maoGrappleable中的文本信息，赋予不同编号的Tile是否可以被钩住
        for(int i=0; i<gTiles.Length; i++) {
            switch(gTiles[i]) {
                case 'S':
                    grappleTiles.Add(i);
                    break;
                case 'X':
                    unsafeTiles.Add(i);
                    break;
            }
        }

        //获取各个Component
        dray = GetComponent<Dray>();
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        drayColld = GetComponent<Collider>();

        Transform trans = transform.Find("Grappler");
        grapHead = trans.gameObject;    //获取Grappler预设
        grapLine = grapHead.GetComponent<LineRenderer>();    //获取预设中的LineRenderer组件
        grapHead.SetActive(false);
    }

    void Update() {
        if(!dray.hasGrappler) return;

        switch (mode) {
            case eMode.none:
                if(Input.GetKeyDown(KeyCode.X)) {
                    StartGrapple();
                }
                break;
            
        }
    }

    void StartGrapple() {
        facing = dray.GetFacing();
        dray.enabled = false;
        anim.CrossFade("Dray_Attack_" + facing, 0);
        drayColld.enabled = false;
        rigid.velocity = Vector3.zero;

        grapHead.SetActive(true);

        p0 = transform.position + (directions[facing] * 0.5f);  //使得抓钩有一个相对角色向前的错位
        p1 = p0;
        grapHead.transform.position = p1;
        grapHead.transform.rotation = Quaternion.Euler(0,0,90*facing);

        grapLine.positionCount = 2;
        grapLine.SetPosition(0, p0);
        grapLine.SetPosition(1, p1);

        mode = eMode.gOut;
    }

    void FixedUpdate() {
        switch(mode) {
            case eMode.gOut:
                p1 += directions[facing] * grappleSpd * Time.fixedDeltaTime;
                grapHead.transform.position = p1;    //钩子的位置移动
                grapLine.SetPosition(1, p1);    //绳子渲染长度变化

                //检查是否碰撞到了什么
                int tileNum = TileCamera.GET_MAP(p1.x, p1.y);   //返回对应位置Tile的编号
                //根据编号来检查是否能被钩住
                if(grappleTiles.IndexOf(tileNum) != -1) {
                    mode = eMode.gInHit;  //此Tile被钩住
                    break;
                }
                if((p1 - p0).magnitude >= grappleLength) {
                    mode = eMode.gInMiss;
                }
                break;

            case eMode.gInMiss:
                //没有钩住，两倍速度往回缩
                p1 -= directions[facing] * 2 *grappleSpd * Time.fixedDeltaTime;
                // 如果始终在角色的前面
                if(Vector3.Dot((p1-p0), directions[facing]) > 0) {
                    grapHead.transform.position = p1;
                    grapLine.SetPosition(1, p1);
                } else {
                    StopGrapple();
                }
                break;
            case eMode.gInHit:
                float dist = grappleInLength + grappleSpd * Time.fixedDeltaTime;
                if(dist > (p1-p0).magnitude) {
                    p0 = p1 - (directions[facing] *grappleInLength);
                    transform.position = p0;    // 将角色移动到位置
                    StopGrapple();
                    break;
                }
                p0 += directions[facing] * grappleSpd *Time.fixedDeltaTime;
                transform.position = p0;    //让角色向钩子头的方向飞行
                grapLine.SetPosition(0, p0);    
                grapHead.transform.position = p1;
                break;
        }
    }

    void StopGrapple() {
        dray.enabled = true;
        drayColld.enabled = true;
        int tileNum = TileCamera.GET_MAP(p0.x, p0.y);

        if(mode == eMode.gInHit && unsafeTiles.IndexOf(tileNum) != -1) {
            dray.ResetInRoom(unsafeTileHealthPenalty);
        }

        grapHead.SetActive(false);
        mode = eMode.none;
    }

    void OnTriggerEnter(Collider colld) {
        Enemy e = colld.GetComponent<Enemy>();
        if(e == null) return;
        mode = eMode.gInMiss;
    }
}