using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//製作地圖
public class Mathfff : MonoBehaviour
{
    /// <summary>
    /// 真實地圖
    /// </summary>
    public GameObject[] map =
       {
        new GameObject ("mapA"),
        new GameObject ("mapB"),
        new GameObject ("mapC"),
        new GameObject ("mapD"),
        new GameObject ("mapE"),
    };

    /// <summary>
    /// 地圖類型
    /// </summary>
    public GameObject[] smallMap =
       {
        new GameObject ("smallMapA"),
        new GameObject ("smallMapB"),
        new GameObject ("smallMapC"),
        new GameObject ("smallMapD"),
        new GameObject ("smallMapE"),
    };
    //格子 位置
    public Vector3[] lattice;

    private Vector2[] smallMapV2;
    private bool[] mapBool;
    private bool[] smallMapBool;
    //地圖大小
    public float mapSize;


    private void Awake()
    {
        for (int i = 0; i < 5; i++)
        {
            map[i] = GetComponent<GameObject>();
            smallMap[i] = GetComponent<GameObject>();
            smallMapV2[i] = smallMap[i].transform.position;
        }
    }

    private void OnPointerDown()
    {
        for (int i = 0; i < 5; i++)
        {
            //在哪個小圖旁邊 點擊,固定選項
            if (Input.GetTouch(0).position.x - smallMapV2[i - 1].x < mapSize || Input.GetTouch(0).position.y - smallMapV2[i - 1].y < mapSize)
            {
                smallMapBool[i - 1] = true;
            }
            //在格子上點擊,如果沒有物件 就產生。排整齊的話,任何i格子都是固定j位置,共36個格子。
            else for (int j = 0; j < 36; j++)
                {
                    if (Input.GetTouch(0).position.x - lattice[j - 1].x < mapSize || Input.GetTouch(0).position.y - lattice[j - 1].y < mapSize ||mapBool[j-1]==false)
                    {
                        //產生(大地圖跟小地圖,座標,角度)
                        Instantiate(smallMap[i - 1], lattice[j - 1], transform.rotation);
                        //200倍的大小
                        Instantiate(map[i - 1], lattice[j - 1] * 200, transform.rotation);
                        mapBool[j-1] = true;
                    }
                    //點擊 空白處取消 選項 
                    else smallMapBool[i - 1] = false;
                }
        }
    }

    private void OnPointerUp()
    {
        for (int i = 0; i < 36; i++)
        {
            if (mapBool[i-1]=true)
            {
                //產生路障

                //如果 地圖(第九層)相接 消除路障
                Physics.Raycast(smallMapV2[i-1],smallMap[i-1].transform.up,mapSize+1,1<<9);

            }
        }

    }

}
