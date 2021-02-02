using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//提升鏡頭跟隨角色 的沉浸感。眼角餘光(光暈)。側面感應(光暈面玩家變色 或 感應環在畫面中央)。
public class MoveJoyStick : MonoBehaviour
{
    #region  欄位
    [Header("搖桿")]
    public GameObject joyStick;
    [Header("搖桿背景圖範圍"), Tooltip("JoyStickBackGround背景圖")]
    public GameObject joyBG;
    [Header("搖桿半徑")]
    public float jyRadiu;
    //搖桿離背景圖中心的距離
    public Vector2 direction;
    //搖桿原點
    Vector2 startPos = Vector2.zero;
    //目前搖桿位置。測試完要設定私人
    public Vector2 joyStickV2;
    //位移後 搖桿位置
    Vector2 joyStickV2Move;
    //位移後 要轉動的向量，以 位移後 搖桿位置 為原點
    Vector2 joyStickMV2;

    [Tooltip("玩家自己")]
    public Rigidbody playerA;
    [Header("角色動畫")]
    public Animation anim;
    [Header("角色速度"), Range(0.5f, 1.2f)]
    public float speed;
    //走路轉動偏移量
    float walkRotate;
    //初始速度
    float startSpeed;
    //搖桿拉開後，跳躍
    bool jump;
    //角色位移後再 轉動值
    private float playerRotate = 0;
    //敵人的武器角度
    float armathB;

    //玩家自己的攝影機 視角
    public Camera playerAC;
    [Header("cameraX")]
    float cameraX = 0;
    [Header("Y在角色眼睛")]
    public float cameraY = 3.15f;
    [Header("Z在手肘向前的位置")]
    public float cameraZ = 0.3f;
    [Header("左右轉動視角")]
    public float cameraRotate;
    [Header("轉頭角度")]
    public float cameraY2;
    //第一人稱

    //是否離開地面
    bool vacate = false;

    //頓下的時間
    float squatTime;

    //最大生命
    static public float healthMax;
    //生命
    static public float health;
    #endregion

    private void Awake()
    {
        anim = GetComponent<Animation>();
        playerAC = GameObject.Find("PlayerACamera").GetComponent<Camera>();
        playerA = GameObject.Find("remy@Withdrawing Sword").GetComponent<Rigidbody>();
        joyBG = GameObject.Find("JoyStickBackGround");
        joyStick = GameObject.Find("JoyStick");
    }

    void Start()
    {
        startPos = joyStick.transform.position;
        startSpeed = speed;
    }

    void Update()
    {
        //蹲下時間 開始計時，0.5秒後重置
        if (squatTime != 0)
        {
            float squatTimeB = Time.deltaTime;
            if (squatTimeB > 0.5f)
            {
                squatTime = 0;
                squatTimeB = 0;
            }
        }

        FirstPersonPerspective();
    }

    void FixedUpdate()
    {
        //偵測是否離開地面
        if (Physics.Raycast(playerA.transform.position, Vector3.forward, 0.1f, 1 >> 8))
        {
            vacate = false;
        }


    }

    #region 方法
    /// <summary>
    ///角色轉身
    /// </summary>
    private void PlayerRotate()
    {
        if (joyStickV2.x == 0 && joyStickV2.y == 0)
        {
            //播放不跨步

            speed = startSpeed;
            return;
        }

        //放開點-開始點 的向量
        joyStickMV2 = joyStick.GetComponent<RectTransform>().anchoredPosition - joyStickV2Move;

        #region Atan2的xy為0,返回正確的角度,而不是拋出被0除的異常
        if (joyStickMV2.x == 0 && joyStickMV2.y > 0)
        {
            playerRotate = 90;
        }
        else if (joyStickMV2.y < 0)
        {
            playerRotate = 270;
        }

        if (joyStickMV2.y == 0 && joyStickMV2.x >= 0)
        {
            playerRotate = 0;
        }
        else
        {
            playerRotate = 180;
        }
        #endregion

        #region 向量轉為角度
        if (joyStickMV2.x != 0 && joyStickMV2.y != 0)
        {
            //向量轉斜率Atan2() * 弧度轉角度 
            //弧度轉角度 為 常數 Rad2Deg =57.29578
            playerRotate = Mathf.Atan2(joyStickMV2.x, joyStickMV2.y) * Mathf.Rad2Deg;
        }
        #endregion

        #region 控制角度在0~360
        if (playerRotate < 0)
        {
            playerRotate += 360;
        }
        if (playerRotate > 360)
        {
            playerRotate -= 360;
        }
        #endregion

        //播放跨完動畫

        //normalized=1,角色再轉向
        if (joyStickMV2.x != 0 && joyStickMV2.y != 0)
        {
            //快速轉身
            playerA.transform.eulerAngles = new Vector3(0, playerRotate, 0);
            speed = startSpeed;
        }
        joyStickV2Move = Vector2.zero;
    }

    /// <summary>
    /// 根據 搖桿與原點的向量 播放 走路動畫
    /// </summary>
    void Walk()
    {
        //放開點-開始點 的向量
        Vector2 noV2 = joyStickV2 - startPos;

        #region Atan2的xy為0,返回正確的角度,而不是拋出被0除的異常
        if (noV2.x == 0 && noV2.y > 0)
        {
            walkRotate = 90;
        }
        else if (noV2.y < 0)
        {
            walkRotate = 270;
        }

        if (noV2.y == 0 && noV2.x >= 0)
        {
            walkRotate = 0;
        }
        else
        {
            walkRotate = 180;
        }
        #endregion

        #region 向量轉為角度
        if (noV2.x != 0 && noV2.y != 0)
        {
            //向量轉斜率Atan2() * 弧度轉角度 
            //弧度轉角度 為 常數 Rad2Deg =57.29578
            walkRotate = Mathf.Atan2(noV2.x, noV2.y) * Mathf.Rad2Deg;
        }
        #endregion

        #region 控制角度在0~360
        if (walkRotate < 0)
        {
            walkRotate += 360;
        }
        if (walkRotate > 360)
        {
            walkRotate -= 360;
        }
        #endregion

        switch (Mathf.Round((walkRotate - 67.5f) / 45))
        {
            case 0:
                anim.Play("walk正前");
                break;
            case 1:
                anim.Play("walk左前");
                break;
            case 2:
                anim.Play("walk正左");
                break;
            case 3:
                anim.Play("walk左下");
                break;
            case 4:
                anim.Play("walk正下");
                break;
            case 5:
                anim.Play("walk右下");
                break;
            case 6:
                anim.Play("walk正右");
                break;
            case 7:
                anim.Play("walk右上");
                break;
        }
    }


    /// <summary>
    /// 跨步
    /// </summary>
    void Stride()
    {
        speed = 0;

        //右腳跨出去
        if (joyStick.transform.position.x > 0)
        {
            if (joyStickV2.x / jyRadiu < 0.5f)
            {
                if (joyStickV2.y / jyRadiu > 0.5f)
                {
                    //1點鐘方向動畫
                    if (joyStick.transform.position.y > 0)
                    {

                        cameraRotate = 30;
                    }
                    //5點鐘方向動畫
                    else
                    {

                        cameraRotate = 150;
                    }
                }
            }
            //2點鐘方向動畫
            else if (joyStick.transform.position.y > 0)
            {

                cameraRotate = 60;
            }
            //4點鐘方向動畫
            else if (joyStick.transform.position.y < 0)
            {

                cameraRotate = 120;
            }
        }
        //左腳跨出去
        if (joyStick.transform.position.x < 0)
        {
            if (joyStickV2.x / jyRadiu < 0.5f)
            {
                if (joyStickV2.y / jyRadiu > 0.5f)
                {
                    //11點鐘方向動畫
                    if (joyStick.transform.position.y > 0)
                    {

                        cameraRotate = -30;
                    }
                    //7點鐘方向動畫
                    else
                    {

                        cameraRotate = -150;
                    }
                }
            }
            //10點鐘方向動畫
            else if (joyStick.transform.position.y > 0)
            {

                cameraRotate = -60;
            }
            //8點鐘方向動畫
            else if (joyStick.transform.position.y < 0)
            {

                cameraRotate = -120;
            }
        }

        //延遲到放開搖桿，才執行PlayerRotate()
        playerRotate = 1;
    }

    /// <summary>
    /// 第一人稱視角，轉頭未實用，要調整cameraY2
    /// </summary>
    void FirstPersonPerspective()
    {
        //視角調整，Y在角色眼睛，Z在手肘向前的位置
        Vector3 cameraV3 = new Vector3(cameraX, cameraY, cameraZ);
        //相機 跟隨 角色
        playerAC.transform.position = playerA.transform.position + cameraV3;
        //注視 角色頭頂。3.15f在角色鼻子，0.6f角色前方
        //第一版的 playerAC.transform.LookAt(new Vector3(playerA.transform.position.x, playerA.transform.position.y + cameraY - 0.05f, playerA.transform.position.z +0.6f));
        playerAC.transform.LookAt(new Vector3(playerA.transform.position.x, playerA.transform.position.y + cameraY, playerA.transform.position.z));
        //轉視角 down繞者下方中央軸旋轉。額外加轉身的角度。
        playerAC.transform.RotateAround(playerA.transform.position, Vector3.down, cameraRotate);
        //正眼看自己+180度 為 自己的視角。轉頭未實用，要調整cameraY2
        playerAC.transform.eulerAngles += new Vector3(0, 180 + cameraY2, 0);
        //播放轉頭動畫
    }
    #endregion


    #region 事件
    /// <summary>
    /// 玩家碰撞玩家，被擊退。玩家被武器攻擊，扣生命。
    /// </summary>
    /// <param name="playerB"></param>
    void OnColliderEnter(Collider playerB)
    {
        if (playerB.gameObject.tag == "玩家")
        {
            speed = 0;
            anim.Play("back");
            if (anim["back"].normalizedSpeed==1)
            {
                speed = startSpeed;
            }
        }

        if (playerB.gameObject.tag == "武器")
        {
            health -= 1;

        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        joyStick.transform.position = Input.GetTouch(0).position;
        //搖桿與原點 的向量
        joyStickV2 = (Vector2)joyStick.transform.position - startPos;
        //紀錄 位移後 位置
        joyStickV2Move = Input.GetTouch(0).position;

        //格擋時不能移動，並且有 敵人的攻擊角度armathB
        if (anim["back"].normalizedSpeed != 0 && anim["back"].normalizedSpeed != 1)
        {
            if (speed == 0)
            {
                //跨步擊潰 左邊來的攻擊
                if ((180 - armathB) > 90 && (180 - armathB) < 270)
                {
                    if (joyStickV2Move.x < 0 && joyStickV2Move.y > 0)
                    {
                        Stride();
                    }
                }
                //跨步擊潰 右邊來的攻擊
                else if ((180 - armathB) <= 90 && (180 - armathB) >= 270)
                {
                    if (joyStickV2Move.x > 0 && joyStickV2Move.y > 0)
                    {
                        Stride();
                    }
                }
            }
        }

        #region 蹲下開關
        if ((joyStickV2 / jyRadiu).magnitude < 0.3)
        {
            //站立
            if (squatTime < 0)
            {
                anim.Play("站立");
                squatTime = 0;
            }
            if (anim["蹲下"].normalizedSpeed == 1)
            {
                //準備 站立 時間
                squatTime -= Time.deltaTime;
            }
            //蹲下
            if (squatTime > 0)
            {
                anim.Play("蹲下");
                squatTime = 0;
            }
            if (anim["蹲下"].normalizedSpeed == 0)
            {
                //準備 蹲下 時間
                squatTime += Time.deltaTime;
            }
        }
        #endregion

        if (speed > 0)
        {
            Stride();
        }
    }

    public void OnMove(PointerEventData eventData)
    {
        //蹲下與站立的時間 拖動就歸零
        squatTime = 0;
        joyStick.transform.position = Input.GetTouch(0).position;
        joyStickV2 = (Vector2)joyStick.transform.position - startPos;

        if (anim["蹲下"].normalizedSpeed == 0)
        {
            //根據 Y左右微轉彎
            playerAC.transform.eulerAngles += new Vector3(0, joyStickV2.x / jyRadiu, 0);
            //蹲下走路
            playerA.velocity = joyStick.transform.position * (Vector2)playerAC.transform.forward * 0.7f*speed;
            anim.Play("蹲下走");
        }
        else if (joyStickV2.x/jyRadiu>0.6f||joyStickV2.y/jyRadiu>0)
        {
            //根據 Y左右微轉彎
            playerAC.transform.eulerAngles += new Vector3(0, joyStickV2.x / jyRadiu, 0);
            //跑步方向同步攝影機
            playerA.velocity = joyStick.transform.position * (Vector2)playerAC.transform.forward * 1.5f*speed;
            anim.Play("run");
            jump = true;
        }
        else 
        {
            //走路方向同步攝影機
            playerA.velocity = joyStick.transform.position * (Vector2)playerAC.transform.forward*speed;
            Walk();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //跳躍
        if (!vacate && jump)
        {
            playerA.velocity = new Vector3(0, 2, 0);
            jump = false;
        }

        //跨步後的 轉身
        if (playerRotate > 0)
        {
            PlayerRotate();
            playerRotate = 0;
        }

    }
    #endregion
}