using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dray : MonoBehaviour, IFacingMover, IKeyMaster
{
    public enum eMode {idle, move, attack, transition, knockback}

    [Header("Set in Inspector")]
    public float speed = 5f;     // m/s
    public float attackDuration = 0.25f;    // 每轮攻击动作所用的时间
    public float attackDelay = 0.5f;    //每次攻击动作之间的间隔
    public float transitionDelay = 0.5f;    //进入不同房间的延迟间隔时间

    public int maxHealth = 10;
    public float knockbackSpeed = 10;
    public float knockbackDuration = 0.25f;
    public float invincibleDuration = 0.5f;

    [Header("Set Dynamically")]
    public int dirHeld = -1;    //按住的方向键所指向的方向
    public int facing = 1;    //人物所朝的方向
    public eMode mode = eMode.idle;

    public int numKeys = 0;
    public bool invincible = false;
    public bool hasGrappler = false;
    public Vector3 lastSafeLoc;
    public int lastSafeFacing;
    
    private float timeAtkDone = 0;    // 攻击动作完成的时间
    private float timeAtkNext = 0;    // 下次攻击动作开始的时间
    private float transitionDone = 0;
    private Vector2 transitionPos;

    private float knockbackDone = 0;
    private float invincibleDone = 0;
    private Vector3 knockbackVel;

    private Rigidbody rigid;
    private Animator anim;
    private InRoom inRm;    //InRoom脚本(组件)
    private Vector3[] directions = new Vector3[] {Vector3.right, Vector3.up, Vector3.left, Vector3.down};
    private SpriteRenderer sRend;    //用来控制Dray受到伤害和不受到伤害时的身体的颜色

    [SerializeField]
    private int _health;

    public int health {
        get{ return _health;}
        set{ _health = value;}
    }

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        inRm = GetComponent<InRoom>();    // this will ensure we can asses the InRoom class;
        sRend = GetComponent<SpriteRenderer>();
        health = maxHealth;
        lastSafeLoc = transform.position;
        lastSafeFacing = facing;
    }

    void Update()
    {   
        if(invincible && Time.time > invincibleDone) invincible = false;
        sRend.color = invincible ? Color.red: Color.white;
        if(mode == eMode.knockback) {
            rigid.velocity = knockbackVel;
            if(Time.time < knockbackDone) return;    //如果还没到被击退动作的结束时间，则不继续执行下面的语句
        }
    
        if(mode == eMode.transition) {
            rigid.velocity = Vector3.zero;
            anim.speed = 0;
            roomPos = transitionPos;   
            if(Time.time < transitionDone) return;
            mode = eMode.idle;
        }

        //根据所按方向键控制移动的方向
        dirHeld = -1;
        if(Input.GetKey(KeyCode.RightArrow)) dirHeld = 0;
        if(Input.GetKey(KeyCode.UpArrow)) dirHeld = 1;
        if(Input.GetKey(KeyCode.LeftArrow)) dirHeld = 2;
        if(Input.GetKey(KeyCode.DownArrow)) dirHeld = 3;
        
        // 按下攻击指令键，并且要求时间一定超过每轮攻击的间隔时间，才开始攻击动作
        if(Input.GetKeyDown(KeyCode.Z) && Time.time >= timeAtkNext) {
            mode = eMode.attack;
            timeAtkDone = Time.time + attackDuration;
            timeAtkNext = Time.time + attackDelay;
        }

        //到了timeAtkDone的时间，结束攻击动作
        if(Time.time >= timeAtkDone) {
            mode = eMode.idle;
        }

        //如果没有在攻击模式，选择合适的模式
        if(mode != eMode.attack) {
            if(dirHeld == -1) {
                mode = eMode.idle;
            } else {
                facing = dirHeld;
                mode = eMode.move;
            }
        }

        //根据当前mode的行为，设置当前mode的animation
        Vector3 vel = Vector3.zero;
        switch (mode) {
            case eMode.attack:
                anim.CrossFade("Dray_Attack_" + facing, 0);
                anim.speed = 0;
                break;
            case eMode.idle:
                anim.CrossFade("Dray_Walk_" + facing, 0);
                anim.speed = 0;
                break;
            case eMode.move:
                vel = directions[dirHeld];
                anim.CrossFade("Dray_Walk_" + facing, 0);
                anim.speed = 1;
                break;
        }

        rigid.velocity = vel * speed;
    }

    void LateUpdate() {
        Vector2 rPos = GetRoomPosOnGrid(0.5f);    //应为DOORS是0.5 grid
        int doorNum;
        //检查是否在door的贴图上，如果不在door贴图位置，则doorNum=4，下句返回不做处理
        //如果在door的贴图位置，则doorNum=0-3，并根据具体哪个数字，决定rm.x, rm.y
        for(doorNum = 0; doorNum < 4; doorNum++) {
            if(rPos == InRoom.DOORS[doorNum]) {
                break;
            }
        }

        if(doorNum > 3 || doorNum != facing) return;

        //移动到另一个房间
        Vector2 rm = roomNum;
        switch(doorNum) {
            case 0:
                rm.x += 1;
                break;
            case 1:
                rm.y += 1;
                break;
            case 2:
                rm.x -= 1;
                break;
            case 3:
                rm.y -= 1;
                break;
        }

        //确保移动到的地图是正确的, 确保角色不移动出地图roomNum.y = -1
        if(rm.x >= 0 && rm.x <= InRoom.MAX_RM_X) {
            if(rm.y >= 0 && rm.y <= InRoom.MAX_RM_Y) {
                roomNum = rm;
                transitionPos = InRoom.DOORS[(doorNum + 2) % 4];    //从不同门穿隧过去的房间的相对位置
                roomPos = transitionPos;
                lastSafeLoc = transform.position;
                lastSafeFacing = facing;
                mode = eMode.transition;
                transitionDone = Time.time + transitionDelay;
            }
        }
    }

    void OnCollisionEnter(Collision coll) {
        if(invincible) return;
        DamageEffect dEf = coll.gameObject.GetComponent<DamageEffect>();    //获取碰撞到的对象coll的组件DamageEffect
        if(dEf == null) return;    //如果碰撞到的对象没有DamageEffect组件，说明该对象不会产生影响

        health -= dEf.damage;
        if(health <= 0) {    //如果角色血量为0，已死亡
            gameObject.SetActive(false);
        }
        invincible = true;
        invincibleDone = Time.time + invincibleDuration;
        
        if(dEf.knockback) {
            //被击退的方向
            Vector3 delta = transform.position - coll.transform.position;  // 四个方向
            if(Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)) {    // 首先查看是水平还是垂直方向
                delta.x = (delta.x > 0) ? 1:-1;
                delta.y = 0;
            } else { 
                delta.x = 0;
                delta.y = (delta.y > 0) ? 1:-1;
            }

            // delta实际是方向，根据方向和击退速度，赋予刚体速度
            knockbackVel = delta*knockbackSpeed;
            rigid.velocity = knockbackVel;

            mode = eMode.knockback;
            knockbackDone = Time.time + knockbackDuration;
        }
    }

    void OnTriggerEnter(Collider colld) {
        PickUp  pup = colld.GetComponent<PickUp>();
        if(pup == null) return;

        switch (pup.itemType) {
            case PickUp.eType.health:
                health = Mathf.Min(health +2, maxHealth);
                break;
            case PickUp.eType.key:
                keyCount++;
                break;
            case PickUp.eType.grappler:
                hasGrappler = true;
                break;
        }
        Destroy(colld.gameObject);
    }

    public void ResetInRoom(int healthLoss = 0) {
        transform.position = lastSafeLoc;
        facing = lastSafeFacing;
        health -= healthLoss;
        if(health <= 0) {    //如果角色血量为0，已死亡
            gameObject.SetActive(false);
        }

        invincible = true;
        invincibleDone = Time.time + invincibleDuration;
    }

    public int GetFacing() {
        return facing;
    }

    public bool moving {
        get { return (mode == eMode.move);}
    }

    public float GetSpeed() {
        return speed;
    }

    public float gridMult {
        get { return (inRm.gridMult);}
    }

    public Vector2 roomPos {
        get { return inRm.roomPos;}
        set { inRm.roomPos = value;}
    }

    public Vector2 roomNum {
        get { return inRm.roomNum; }
        set { inRm.roomNum = value; }
    }

    public Vector2 GetRoomPosOnGrid(float mult = -1) {
        return inRm.GetRoomPosOnGrid(mult);
    }

    public int keyCount {
        get { return numKeys; }
        set { numKeys = value; }
    }
}
