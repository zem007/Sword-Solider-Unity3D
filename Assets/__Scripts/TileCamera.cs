using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Set in Inspector")]
    public TextAsset mapData;
    public Texture2D mapTiles;
    public TextAsset mapCollisions;
    public Tile tilePrefab;

    void Awake() {
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
                    ti.SetTile(i, j);
                    TILES[i, j] = ti;
                }
            }
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
