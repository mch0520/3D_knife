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

    //模式:沒點擊、搖桿區域內點擊、搖桿區域外點擊
    private int mode = 0;
    //搖桿外點擊初始位置
    Vector2 startTouch;
    //搖桿外點擊放開位置
    Vector2 outTouch;

    [Tooltip("玩家自己")]
    public Rigidbody playerA;
    [Header("角色動畫")]
    public Animation anim;
    [Header("角色速度"), Range(0, 1000)]
    public float speed = 300;
    //走路轉動偏移量
    float walkRotate;
    //初始速度
    float startSpeed;
    //搖桿拉開後，跳躍
    bool jump;
    //角色位移後再 轉動開關
    private bool playerTurn = false;
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
    [Header("轉頭攝影機角度")]
    private Vector3 cameraY2;
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

    //判斷是否 落下地板，要調整
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
        if (joyStickMV2.x != 0 || joyStickMV2.y != 0)
        {
            //快速轉身剛體
            playerA.transform.eulerAngles += new Vector3(0, playerRotate, 0);
            //快速轉身攝影機
            playerAC.transform.eulerAngles += new Vector3(0, playerRotate, 0);
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
                    if (joyStick.transform.position.y > 0)
                    {
                        //1點鐘方向動畫  30度
                    }
                    else
                    {
                        //5點鐘方向動畫  150度
                    }
                }
            }
            else if (joyStick.transform.position.y > 0)
            {
                //2點鐘方向動畫  60度
            }
            else if (joyStick.transform.position.y < 0)
            {
                //4點鐘方向動畫  120度
            }
        }
        //左腳跨出去
        if (joyStick.transform.position.x < 0)
        {
            if (joyStickV2.x / jyRadiu < 0.5f)
            {
                if (joyStickV2.y / jyRadiu > 0.5f)
                {
                    if (joyStick.transform.position.y > 0)
                    {
                        //11點鐘方向動畫  -30度
                    }
                    else
                    {
                        //7點鐘方向動畫  -150度
                    }
                }
            }
            else if (joyStick.transform.position.y > 0)
            {
                //10點鐘方向動畫  -60度
            }
            else if (joyStick.transform.position.y < 0)
            {
                //8點鐘方向動畫 -120度
            }
        }

        //延遲到放開搖桿，才執行PlayerRotate()
        playerTurn = true;
    }

    /// <summary>
    /// 第一人稱視角，轉頭未實用，要調整
    /// </summary>
    void FirstPersonPerspective()
    {
        //視角調整，Y在角色眼睛，Z在手肘向前的位置
        Vector3 cameraV3 = new Vector3(cameraX, cameraY, cameraZ);
        //相機 跟隨 角色
        playerAC.transform.position = playerA.transform.position + cameraV3;
        /**注視 角色頭頂。3.15f在角色鼻子，0.6f角色前方
        第一版的 playerAC.transform.LookAt(new Vector3(playerA.transform.position.x, playerA.transform.position.y + cameraY - 0.05f, playerA.transform.position.z +0.6f)); **/
        playerAC.transform.LookAt(new Vector3(playerA.transform.position.x, playerA.transform.position.y + cameraY, playerA.transform.position.z));
        //轉視角 down繞者下方中央軸旋轉。額外加轉身的角度。
        playerAC.transform.RotateAround(playerA.transform.position, Vector3.down, cameraY2.x);
        //轉視角 right繞者右邊中央軸旋轉。額外加轉身的角度。
        playerAC.transform.RotateAround(playerA.transform.position, Vector3.right, cameraY2.y);
        //正眼看自己+180度 為 自己的視角。
        playerAC.transform.eulerAngles += new Vector3(0, 180, 0);
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
            if (anim["back"].normalizedSpeed == 1)
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
        //如果 射線檢測 點擊 在搖桿區域內 才能使用
        if (Mathf.Abs((Input.GetTouch(0).deltaPosition).x - startPos.x) < jyRadiu||Mathf.Abs((Input.GetTouch(0).deltaPosition).y - startPos.y) < jyRadiu)
        {
            joyStick.transform.position = Input.GetTouch(0).position;
            //搖桿與原點 的向量
            joyStickV2 = (Vector2)joyStick.transform.position - startPos;
            //紀錄 位移後 位置
            joyStickV2Move = Input.GetTouch(0).position;
            mode = 1;
        }
        else
        {
            //紀錄第一次點擊位置
            startTouch = Input.GetTouch(0).position;
            mode = 2;
        }
        if (mode == 1)
        {
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
    }

    public void OnMove(PointerEventData eventData)
    {
        if (mode == 1)
        {
            //一拖曳 蹲下與站立的時間 就歸零
            squatTime = 0;

            joyStick.transform.position = Input.GetTouch(0).position;
            joyStickV2 = (Vector2)joyStick.transform.position - startPos;
            //搖桿座標歸一化
            float x = joyStickV2.x / jyRadiu;
            float y = joyStickV2.y / jyRadiu;

            if (anim["蹲下"].normalizedSpeed == 0)
            {
                //角色根據 搖桿Y左右轉彎
                playerAC.transform.eulerAngles += new Vector3(0, Mathf.Clamp(x, 0, 30), 0);
                //角色物件 跟 攝影機 同向
                playerA.transform.forward = playerAC.transform.forward;
                //蹲下走路。角色根據攝影機方向 相對移動
                playerA.velocity = (x * playerAC.transform.right + y * playerAC.transform.forward) * 0.6f * speed;
                anim.Play("蹲下走");
                jump = false;
            }
            else if (x > 0.6f || y > 0)
            {
                //角色根據 搖桿Y左右轉彎
                playerAC.transform.eulerAngles += new Vector3(0, Mathf.Clamp(x, 0, 30), 0);
                //角色物件 跟 攝影機 同向
                playerA.transform.forward = playerAC.transform.forward;
                //跑步。角色根據攝影機方向 相對移動
                playerA.velocity = (x * playerAC.transform.right + y * playerAC.transform.forward) * 1.8f * speed;
                anim.Play("run");
                jump = true;
            }
            else
            {
                //走路方向同步攝影機
                playerA.velocity = (x * playerAC.transform.right + y * playerAC.transform.forward) * speed;
                Walk();
                jump = false;
            }
        }

        if (mode == 2)
        {
            outTouch = Input.GetTouch(0).position;
            //搖桿歸一化
            cameraY2.x=(outTouch.x - startTouch.x) / jyRadiu*70 ;
            cameraY2.y=(outTouch.y - startTouch.y)/jyRadiu*30 ;

            Mathf.Clamp(cameraY2.x,-70,70);
            Mathf.Clamp(cameraY2.y,-30,30);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (mode == 1)
        {
            //跳躍
            if (!vacate && jump)
            {
                playerA.velocity += new Vector3(0, 20, 0);
                jump = false;
            }

            //跨步後的 轉身
            if (playerTurn)
            {
                PlayerRotate();
                playerTurn = false;
                speed = startSpeed;
            }
        }
    }
    #endregion
}