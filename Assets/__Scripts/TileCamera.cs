using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileSwap {
    public int tileNum;
    public GameObject swapPrefab;
    public GameObject guaranteedItemDrop;
    public int overrideTileNum = -1;
}

//这个脚本是用来编译和储存所有sprites的
//所有sprites从DelverTiles.psd中读取
//而具体的每个sprite所属的位置是从DelverData中读取
public class TileCamera : MonoBehaviour
{
    private static int W,H;
    private static int[,] MAP;
    public static Sprite[] SPRITES;
    public static Transform TILE_ANCHOR;
    public static Tile[,] TILES;
    public static string COLLISIONS;

    [Header("Set in Inspector")]
    public TextAsset mapData;
    public Texture2D mapTiles;
    public TextAsset mapCollisions;
    public Tile tilePrefab;
    public int defaultTileNum;
    public List<TileSwap> tileSwaps;

    private Dictionary<int, TileSwap> tileSwapDict;
    private Transform enemyAnchor, itemAnchor;

    void Awake() {
        COLLISIONS = Utils.RemoveLineEndings(mapCollisions.text);
        PrepareTileSwapDict();
        enemyAnchor = (new GameObject("Enemy Anchor")).transform;
        itemAnchor = (new GameObject("Item Anchor")).transform;
        LoadMap();
    }

    public void LoadMap() 
    {
        //创建一个所有Tile的父对象TILE_ANCHOR实例
        GameObject go = new GameObject("TILE_ANCHOR");
        TILE_ANCHOR = go.transform;

        //从mapTiles中load所有的sprites
        SPRITES = Resources.LoadAll<Sprite>(mapTiles.name);

        //读取mapData中的信息，每行每行的读取
        string[] lines = mapData.text.Split('\n');
        H = lines.Length;
        string[] tileNums = lines[0].Split(' ');    //每行中的Tile数量
        W = tileNums.Length;

        System.Globalization.NumberStyles hexNum;
        hexNum = System.Globalization.NumberStyles.HexNumber;

        //MAP是个二维数组，储存了所有mapData中的所有16进制编号的位置
        MAP = new int[W, H];
        for(int j=0; j<H; j++) {
            tileNums = lines[j].Split(' ');
            for(int i=0; i<W; i++) {
                if(tileNums[i] == "..") {
                    MAP[i, j] = 0;
                } else {
                    MAP[i, j] = int.Parse(tileNums[i], hexNum);
                }
                CheckTileSwaps(i, j);
            }
        }
        print("Parsed " + SPRITES.Length + "sprites.");
        print("Map size: " + W + "wide by " + H + "high");

        ShowMap();
    }

    //根据Map中储存的信息，调用SetTile方法
    void ShowMap() 
    {
        TILES = new Tile[W,H];
        for(int i=0; i<W; i++) {
            for(int j=0; j<H; j++) {
                if(MAP[i,j] != 0) {
                    Tile ti = Instantiate<Tile>(tilePrefab);
                    ti.transform.SetParent(TILE_ANCHOR);
                    // ti.SetTile(i, j);

                    int tN = MAP[i, j];
                    if(tileSwapDict.ContainsKey(tN)) {
                        ti.SetTile(i, j, defaultTileNum);
                    } else {
                        ti.SetTile(i, j);
                    }
                    
                    TILES[i, j] = ti;
                }
            }
        }
    }

    void PrepareTileSwapDict() {
        tileSwapDict = new Dictionary<int, TileSwap>();
        foreach(TileSwap ts in tileSwaps) {
            tileSwapDict.Add(ts.tileNum, ts);
        }
    }

    //对于(i,j)坐标处的Tile，如果对应的原始Tile是tileSwapDict的key
    //说明应该被新的Tile替换，Tile是Enemy或者Item，并初始化
    void CheckTileSwaps(int i, int j) {
        int tNum = GET_MAP(i,j);
        if(!tileSwapDict.ContainsKey(tNum)) return;
        TileSwap ts = tileSwapDict[tNum];
        if(ts.swapPrefab != null) {
            GameObject go = Instantiate(ts.swapPrefab);
            Enemy e = go.GetComponent<Enemy>();
            if(e != null) {
                go.transform.SetParent(enemyAnchor);
            } else {
                go.transform.SetParent(itemAnchor);
            }
            go.transform.position = new Vector3(i,j,0);
            if(ts.guaranteedItemDrop != null) {
                if(e != null) {
                e.guaranteedItemDrop = ts.guaranteedItemDrop;
                }
            }
        }
        //当前(i,j)位置的Tile被目标编号的Tile替换
        if(ts.overrideTileNum == -1) {
            SET_MAP(i, i, defaultTileNum);
        } else {
            SET_MAP(i, j, ts.overrideTileNum);
        }
    }

    //该静态方法被Tile脚本调用
    //如果SetTile的eTileNum编号为空，则通过此方法找到MAP中的xy对应的值并返回给eTileNum
    public static int GET_MAP(int x, int y) {
        if(x<0 || x>=W || y<0 || y>=H) {
            return -1;    //不允许超过indexrange
        }
        return MAP[x,y];
    }

    public static int GET_MAP(float x, float y) {
        int tX = Mathf.RoundToInt(x);
        int tY = Mathf.RoundToInt(y - 0.25f);   //允许玩家角色的上半生在Tile贴图之外

        return GET_MAP(tX, tY);
    }

    //这个静态方法是用来设置MAP中某x，y位置的值为tNum
    public static void SET_MAP(int x, int y, int tNum) {
        if(x<0 || x>=W || y<0 || y>=H) {
            return;    //不允许超过indexrange
        }
        MAP[x, y] = tNum;
    }
}
