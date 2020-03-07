using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GuiPanel : MonoBehaviour 
{
    [Header("Set in inspector")]
    public Dray dray;
    public Sprite healthEmpty;
    public Sprite healthHalf;
    public Sprite healthFull;
    
    Text panelNameText;
    Text keyCountText;
    List<Image> healthImages;

    void Start() {
        // GO GO GO~~
        Transform panelName = transform.Find("Title");
        panelNameText = panelName.GetComponent<Text>();
        panelNameText.text = "Good Luck";
        // key count
        Transform trans = transform.Find("Key Count");    //此对象包含一个叫Key Count的Text组件
        keyCountText = trans.GetComponent<Text>();
        // health Icons
        Transform healthPanel = transform.Find("Health Panel");    //此对象包含一个叫Health Panel的组件
        healthImages = new List<Image>();
        if(healthPanel != null) {
            for(int i=0; i<20; i++) {
                trans = healthPanel.Find("H_" + i);     //Health Panel组件又包含了名为H0-H19的Image组件
                if(trans == null) break;
                healthImages.Add(trans.GetComponent<Image>());
            }
        }
    }

    void Update() {
        // show keys
        keyCountText.text = dray.numKeys.ToString();
        // show health
        int health = dray.health;   //调用dray的health属性get
        if(health <= 0) {
            panelNameText.text = "Game Over";
            Invoke("RestartGame", 5);
        }
        for(int i=0; i < healthImages.Count; i++) {
            if(health >= 2) {
                healthImages[i].sprite = healthFull;
            } else if(health == 1) {
                healthImages[i].sprite = healthHalf;
            } else {
                healthImages[i].sprite = healthEmpty;
            }
            health -= 2;   //每两滴血显示一个整格
        }
    }

    void RestartGame() {
        SceneManager.LoadScene("_Scene_Hat");
    }
}
