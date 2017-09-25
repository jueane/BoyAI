using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JumpDetectorBody : MonoBehaviour
{
    public bool showLine;

    BoyAI ai;
    //模拟碰撞用的地面，危险，浮空等层。
    int interestingLayer;

    bool wantJump;
    public bool isCollided;
    public bool isSurvive;
    public float usedSpeed;

    public float simulateMoveX;
    public float simulateMoveY;

    public int collideType;

    Vector3 centerOffset = Vector3.up;

    //帧间隔
    float deltaTime = 0.033f;






    //平移速度
    public float moveSpeed = 5;
    //水平输入速度
    public float horizontalInputSpeed = 0;


    // 所有碰撞层
    int layerCollision;
    // 仅地面
    int layerGround;

    //可移动方向检测结果
    public bool moveToLeft;
    public bool moveToRight;
    public bool moveToUp;
    public bool moveToDown;
    //剩余最大移动距离
    public float leftDis;
    public float rightDis;
    public float upDis;
    // 横向检测长度
    float horizontalCheckDis = 0.4f;


    //角色状态
    public BoyState state;


    //平衡法线
    public Vector3 midNormal;
    public float slope;
    //打线的起始位置与地面的距离
    const float originPoint2Ground = 1f;
    //与地面的距离（小于0表示未打到或超过贴合距离）
    public float disToGround;
    public float disToGround1;
    public float disToGround2;
    bool isFootHit;
    bool isFootHit1;
    bool isFootHit2;
    public float angleToGround;
    public float angleToGround1;
    public float angleToGround2;
    //脚线是否命中冰面
    public bool hasHitIceGround = false;





    //最大摩擦斜率（滑落、斜向上跑）
    float maxFrictionSlope = 0.5f;
    //滑落速度
    public float slideSpeed = 7.5f;
    //冰面滑落速度
    public float slideSpeedOnIce = 8.75f;
    //滑落方向（取平衡向量的垂直向量）
    Vector3 slideVector;






    // 是否可以操作
    public bool CanMoveFreely = true;
    // 是否可以水平移动
    public bool CanHorizontalMove = true;
    // 是否可以垂直移动
    public bool CanVerticalMove = true;


    // 可移动最大角度
    public float moveMaxAngle = 50f;






    //跳跃速度
    public float jumpSpeed = 5;
    //实时速度[跳跃过程中的速度]
    public float jumpInstantSpeed = 0;
    //用力跳[长按空格跳的更高]幅度
    public float jumpHigher = 0.4f;
    //上升衰减速度
    public float jumpAttenuation = 13;



    //下落射线长度（红线）
    float rayLen = 1.5f;

    //下落速度
    public float fallSpeed = 0;
    //下落加速度
    public float fallAccelerateSpeed = 25;
    //最大下落速度
    public float maxFallSpeed = 18;

    public float jumpspeed = 5;



    // Use this for initialization
    void Start()
    {
    }

    //创建的时候初始化
    public void Init()
    {
        //经测试，Init放到start里边不能及时执行。因为此对象是在一帧间隔中创建，会在这帧的Update最后（LateUpate之前）调用start。
        ai = GameManager.Instance.boy.GetComponent<BoyAI>();

        interestingLayer = LayerMask.GetMask(LayerName.ground, LayerName.Platform, LayerName.Danger, LayerName.Default, LayerName.Floating);
        layerCollision = LayerMask.GetMask(LayerName.ground, LayerName.Platform);
        layerGround = LayerMask.GetMask(LayerName.ground);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Jump(float speed)
    {
        wantJump = true;
        state = BoyState.Raising;
        horizontalInputSpeed = speed;
        jumpInstantSpeed = jumpspeed;


        for (int i = 0; i < 100; i++)
        {
            //危险和安全机关算是collided，地形不算collided.
            if (this.isCollided)
            {
                break;
            }
            JumpSimulate();
        }


        CheckSurvive();
    }

    //跳跃模拟
    void JumpSimulate()
    {
        Raise();

        FallCheck();

        Fall();

        Slide();

        KeepMoving();


        //碰撞模拟
        CollideSimulate();
    }

    //碰撞模拟
    void CollideSimulate()
    {

        Collider[] cArr = Physics.OverlapBox(transform.position, ai.boy.ColliderSize / 1.8f, Quaternion.identity, interestingLayer);

        bool collideDanger = false;
        bool collideSecure = false;

        for (int i = 0; i < cArr.Length; i++)
        {
            Collider c = cArr[i];
            if (c.gameObject.layer == LayerMask.NameToLayer(LayerName.Danger))
            {
                collideType = 1;
                collideDanger = true;
            }

            if (c.gameObject.layer == LayerMask.NameToLayer(LayerName.Floating))
            {
                collideType = 2;
                collideSecure = true;
            }

            NewClingWallTrigger cwt = c.GetComponent<NewClingWallTrigger>();
            if (cwt)
            {
                if (cwt.IsRight == ai.IsFollowingRight())
                {
                    collideType = 3;
                    collideSecure = true;
                }
            }

            Ladder ladder = c.GetComponent<Ladder>();
            if (ladder && Mathf.Abs(c.transform.position.x - ladder.transform.position.x) < ladder.PlayerClimbDis)
            {
                if ((ladder.ClimbingDirection == 6 && ai.IsFollowingRight()) || ladder.ClimbingDirection == 4 && ai.IsFollowingRight() == false)
                {
                    collideType = 4;
                    collideSecure = true;
                }
            }

            TransferDoor door = c.GetComponent<TransferDoor>();
            if (door)
            {
                collideType = 5;
                collideSecure = true;
            }
        }

        //循环检测结束
        if (collideDanger)
        {
            jumpInstantSpeed = 0;
            horizontalInputSpeed = 0;

            isCollided = true;
            isSurvive = false;
        }
        else if (collideSecure)
        {
            jumpInstantSpeed = 0;
            horizontalInputSpeed = 0;

            isCollided = true;
            isSurvive = true;
        }

    }

    //检查生还【落点还是地面】
    void CheckSurvive()
    {
        //必须行检查是否已经碰撞完成[isCollided == false]
        if (isCollided == false && jumpInstantSpeed == 0 && state == BoyState.Grounded)
        {
            //float dis = Vector3.Distance(transform.position, ai.boy.transform.position + centerOffset);
            simulateMoveX = transform.position.x - (ai.boy.transform.position + centerOffset).x;
            simulateMoveY = transform.position.y - (ai.boy.transform.position + centerOffset).y;

            //如果斜率过大
            if (slope >= 0.5f)
            {
                jumpInstantSpeed = 0;
                horizontalInputSpeed = 0;


                isCollided = true;
                isSurvive = false;
            }
            else if (simulateMoveX > 2f || Mathf.Abs(simulateMoveY) > 0.4f)
            {
                collideType = 6;
                //落点是地面的，且距离起点过近的，当作无效落点。（攀登，或跨越）

                jumpInstantSpeed = 0;
                horizontalInputSpeed = 0;


                isCollided = true;
                isSurvive = true;
            }
            else
            {
                jumpInstantSpeed = 0;
                horizontalInputSpeed = 0;


                isCollided = true;
                isSurvive = false;
            }

        }
    }


















    //执行跳的过程
    void Raise()
    {
        //上升阶段
        if (jumpInstantSpeed > 0)
        {
            float raiseDis = deltaTime * jumpInstantSpeed;
            raiseDis += raiseDis * jumpHigher;
            transform.position += Vector3.up * raiseDis;


            //上升速度递减
            jumpInstantSpeed -= jumpAttenuation * deltaTime;
            if (jumpInstantSpeed < 0)
            {
                jumpInstantSpeed = 0;
            }
        }
        else
        {
            if (wantJump && jumpInstantSpeed == 0 && state == BoyState.Raising)
            {
                state = BoyState.Falling;
                wantJump = false;
                fallSpeed = 0;
            }
        }
    }

    void Fall()
    {
        if (state != BoyState.Falling)
        {
            return;
        }

        //当前帧下落距离
        float fallDis = fallSpeed * deltaTime;

        //下落状态判断（不在地面的情况）[判断!=0即只判断射线打中地面的情况，距离不够或已超过的情况不在此考虑]
        if (disToGround != 0)
        {



            if (isFootHit == false && isFootHit1 == false && isFootHit2 == false)
            {
                transform.position += Vector3.down * fallDis;
            }
            else
            {
                if (isFootHit)
                {
                    DoFall(fallDis, disToGround);
                }
                else if (isFootHit1)
                {
                    DoFall(fallDis, disToGround1);
                }
                else if (isFootHit2)
                {
                    DoFall(fallDis, disToGround2);
                }
            }
        }
        if (fallSpeed < maxFallSpeed)
        {
            fallSpeed += fallAccelerateSpeed * deltaTime;
        }
    }

    void DoFall(float fallDis, float dis)
    {
        if (fallDis < dis)
        {
            //print("下落中");
            //速度=falldis*斜率。坡度越大，速度越慢。
            transform.position += Vector3.down * fallDis;
        }
        else if (fallDis == dis)
        {
            //print("将要接触");
            transform.position += Vector3.down * fallDis;
        }
        else if (fallDis > dis)
        {
            //			print("将要超过"+dis);
            transform.position += Vector3.down * dis;
        }
    }


    void KeepMoving()
    {
        if (IsOnGround())
        {
            return;
        }

        MoveCheck();
        float _moveDis = 0;
        //计算移动距离（如果按左右方向键，则设置速度，否则速度递减）
        if (horizontalInputSpeed != 0)
        {
            _moveDis = horizontalInputSpeed * deltaTime * moveSpeed;
        }

        if (_moveDis < 0 && moveToLeft)
        {
            transform.Translate(Vector3.right * _moveDis, Space.World);
        }
        if (_moveDis > 0 && moveToRight)
        {
            transform.Translate(Vector3.right * _moveDis, Space.World);
        }
        //进行左右移动（有速度时才移动）

    }

    //是否在地面
    public bool IsOnGround()
    {
        if (state == BoyState.Grounded)
        {
            return true;
        }
        else if (state == BoyState.Falling && Mathf.Abs(GetMinDis()) < 0.1f)
        {
            return true;
        }

        return false;
    }


    //是否在冰面（属于地面）
    public bool isOnIceGround()
    {
        //if (hasHitIceGround && (state == RoleState.Grounded || state == RoleState.Falling) && Mathf.Abs(disToGround) < 0.2f)
        if (hasHitIceGround && (state == BoyState.Grounded || state == BoyState.Falling) && Mathf.Abs(GetMinDis()) < 0.2f)
        {
            return true;
        }
        return false;
    }

    //最接近地面的距离
    float GetMinDis()
    {
        //取命中，且大于-0.4f，的最小值。
        List<float> disList = new List<float>();
        if (isFootHit && disToGround > -originPoint2Ground)
        {
            disList.Add(Mathf.Abs(disToGround));
        }
        if (isFootHit1 && disToGround1 > -originPoint2Ground)
        {
            disList.Add(Mathf.Abs(disToGround1));
        }
        if (isFootHit2 && disToGround2 > -originPoint2Ground)
        {
            disList.Add(Mathf.Abs(disToGround2));
        }

        float minDis = -1;
        if (disList.Count > 0)
        {
            minDis = disList[0];

            for (int i = 0; i < disList.Count; i++)
            {
                if (minDis > disList[i])
                {
                    minDis = disList[i];
                }
            }
        }

        return minDis;
    }


    //滑落（在斜度过大的坡上）
    void Slide()
    {
        //注意！！不能加这个判断（state == RoleState.Grounded），加上会顿挫。

        slideVector = Vector3.zero;

        if (midNormal.x != 0)
        {
            float x = midNormal.y;
            float y = -midNormal.x;
            if (midNormal.x < 0)
            {
                slideVector = new Vector3(-x, -y, 0);
                if (showLine)
                {
                    Debug.DrawRay(transform.position, slideVector, Color.white);
                }
            }
            else if (midNormal.x > 0)
            {
                slideVector = new Vector3(x, y, 0);
                if (showLine)
                {
                    Debug.DrawRay(transform.position, slideVector, Color.white);
                }
            }
        }

        if (state != BoyState.Raising && state != BoyState.Climbing)
        {
            if (slope > maxFrictionSlope)
            {
                //print("下滑.普通");
                transform.Translate(slideVector * slideSpeed * deltaTime, Space.World);
            }
        }
    }




    void MoveCheck()
    {
        moveToLeft = true;
        moveToRight = true;
        moveToUp = true;
        leftDis = 0;
        rightDis = 0;
        upDis = 0;

        //左上（可移动距离1）
        if (showLine)
        {
            Debug.DrawRay(transform.position, new Vector3(-0.5f, 1f, 0) * 0.8f, Color.red);
        }
        RaycastHit hit3;
        bool isHit3 = Physics.Raycast(transform.position, new Vector3(-0.5f, 1f, 0), out hit3, 0.8f, layerGround);
        if (isHit3)
        {
            moveToLeft = false;
            moveToUp = false;
        }

        //中上（可移动距离1.5）
        if (showLine)
        {
            Debug.DrawRay(transform.position, new Vector3(0f, 1f, 0) * 1.5f, Color.red);
        }
        RaycastHit hit4;
        bool isHit4 = Physics.Raycast(transform.position, new Vector3(0f, 1f, 0), out hit4, 1.5f, layerGround);
        if (isHit4)
        {
            upDis = hit4.distance - 1f;
            //注意！这里不能用绝对值，会造成向上穿越。
            if (upDis < 0.001f)
            {
                moveToUp = false;
            }
        }

        //右上（可移动距离1）
        if (showLine)
        {
            Debug.DrawRay(transform.position, new Vector3(0.5f, 1f, 0) * 0.8f, Color.red);
        }
        RaycastHit hit5;
        bool isHit5 = Physics.Raycast(transform.position, new Vector3(0.5f, 1f, 0), out hit5, 0.8f, layerGround);
        if (isHit5)
        {
            moveToUp = false;
            moveToRight = false;
        }

        //中左（可移动距离1）
        if (showLine)
        {
            Debug.DrawRay(transform.position, new Vector3(-1f, 0f, 0) * 1f, Color.red);
        }
        RaycastHit hitMid_Left;
        bool isHitMid_Left = Physics.Raycast(transform.position, new Vector3(-1f, 0f, 0), out hitMid_Left, 1f, layerGround);
        if (isHitMid_Left)
        {
            leftDis = hitMid_Left.distance - horizontalCheckDis;
            if (leftDis < 0.001f)
            {
                moveToLeft = false;

                //if (skillState.BoxPushing)
                //{
                //    if (!skillState.PlayerAtBoxLeft)
                //    {
                //        // 角色在推箱子时，在箱子右边

                //        if (hitMid_Left.collider.CompareTag("PushBox"))
                //        {
                //            moveToLeft = true;
                //        }
                //    }
                //}
            }
        }

        //上左（可移动距离1）
        if (showLine)
        {
            Debug.DrawRay(transform.position + new Vector3(0, 0.9f, 0), new Vector3(-1f, 0f, 0) * 1f, Color.red);
        }
        RaycastHit hitUp_Left;
        bool isHitUp_Left = Physics.Raycast(transform.position + new Vector3(0, 0.9f, 0), new Vector3(-1f, 0f, 0), out hitUp_Left, 1f, layerGround);
        if (isHitUp_Left)
        {
            float tempDis = hitUp_Left.distance - horizontalCheckDis;
            if (tempDis < 0.001f)
            {
                moveToLeft = false;
                //if (skillState.BoxPushing)
                //{
                //    if (!skillState.PlayerAtBoxLeft)
                //    {
                //        // 角色在推箱子时，在箱子右边

                //        if (hitUp_Left.collider.CompareTag("PushBox"))
                //        {
                //            moveToLeft = true;
                //        }
                //    }
                //}
            }

            if (tempDis < leftDis)
            {
                leftDis = tempDis;
            }

        }

        //下左（可移动距离1）
        if (showLine)
        {
            Debug.DrawRay(transform.position + new Vector3(0, -0.7f, 0), new Vector3(-1f, 0f, 0) * 1f, Color.red);
        }
        RaycastHit hitDown_Left;
        bool isHitDown_Left = Physics.Raycast(transform.position + new Vector3(0, -0.7f, 0), new Vector3(-1f, 0f, 0), out hitDown_Left, 1f, layerGround);
        if (isHitDown_Left)
        {

            float angle = VectorAngle(hitDown_Left.normal, Vector3.up);
            if (Mathf.Abs(angle) > moveMaxAngle)
            {

                float tempDis = hitDown_Left.distance - horizontalCheckDis;
                if (tempDis < 0.001f)
                {
                    moveToLeft = false;

                    //if (skillState.BoxPushing)
                    //{
                    //    if (!skillState.PlayerAtBoxLeft)
                    //    {
                    //        // 角色在推箱子时，在箱子右边

                    //        if (hitDown_Left.collider.CompareTag("PushBox"))
                    //        {
                    //            moveToLeft = true;
                    //        }
                    //    }
                    //}
                }

                if (tempDis < leftDis)
                {
                    leftDis = tempDis;
                }
            }

        }

        //中右（可移动距离1）
        if (showLine)
        {
            Debug.DrawRay(transform.position, new Vector3(1f, 0f, 0) * 1f, Color.red);
        }
        RaycastHit hit7;
        bool isHit7 = Physics.Raycast(transform.position, new Vector3(1f, 0f, 0), out hit7, 1f, layerGround);
        if (isHit7)
        {
            rightDis = hit7.distance - horizontalCheckDis;
            if (rightDis < 0.001f)
            {
                moveToRight = false;

                //if (skillState.BoxPushing)
                //{
                //    if (skillState.PlayerAtBoxLeft)
                //    {
                //        // 角色在推箱子时，在箱子左边

                //        if (hit7.collider.CompareTag("PushBox"))
                //        {
                //            moveToRight = true;
                //        }
                //    }
                //}
            }
        }

        //上右（可移动距离1）
        if (showLine)
        {
            Debug.DrawRay(transform.position + new Vector3(0, 0.9f, 0), new Vector3(1f, 0f, 0) * 1f, Color.red);
        }
        RaycastHit hit7_1;
        bool isHit7_1 = Physics.Raycast(transform.position + new Vector3(0, 0.9f, 0), new Vector3(1f, 0f, 0), out hit7_1, 1f, layerGround);
        if (isHit7_1)
        {
            float tempDis = hit7_1.distance - horizontalCheckDis;
            if (tempDis < 0.001f)
            {
                moveToRight = false;
                //if (skillState.BoxPushing)
                //{
                //    if (skillState.PlayerAtBoxLeft)
                //    {
                //        // 角色在推箱子时，在箱子左边

                //        if (hit7_1.collider.CompareTag("PushBox"))
                //        {
                //            moveToRight = true;
                //        }
                //    }
                //}
            }

            if (tempDis < rightDis)
            {
                rightDis = tempDis;
            }
        }

        //下右（可移动距离1）
        if (showLine)
        {
            Debug.DrawRay(transform.position + new Vector3(0, -0.5f, 0), new Vector3(1f, 0f, 0) * 1f, Color.red);
        }
        RaycastHit hit7_2;
        bool isHit7_2 = Physics.Raycast(transform.position + new Vector3(0, -0.5f, 0), new Vector3(1f, 0f, 0), out hit7_2, 1f, layerGround);

        if (isHit7_2)
        {
            //print("命中右下");
            float angle = VectorAngle(hit7_2.normal, Vector3.up);
            if (Mathf.Abs(angle) > moveMaxAngle)
            {


                float tempDis = hit7_2.distance - horizontalCheckDis;
                if (tempDis < 0.001f)
                {
                    moveToRight = false;
                    //if (skillState.BoxPushing)
                    //{
                    //    if (skillState.PlayerAtBoxLeft)
                    //    {
                    //        // 角色在推箱子时，在箱子左边

                    //        if (hit7_2.collider.CompareTag("PushBox"))
                    //        {
                    //            moveToRight = true;
                    //        }
                    //    }
                    //}
                }

                if (tempDis < rightDis)
                {
                    rightDis = tempDis;
                }

            }
        }

        //		horizontalPositionCorrection ();

        if (CanHorizontalMove == false)
        {
            moveToLeft = false;
            moveToRight = false;
        }

    }

    //计算角度（有符号）
    float VectorAngle(Vector3 from, Vector3 to)
    {
        float angle;

        Vector3 cross = Vector3.Cross(from, to);
        angle = Vector3.Angle(from, to);
        return cross.z > 0 ? -angle : angle;
    }

    void FallCheck()
    {
        disToGround = 0;
        disToGround1 = 0;
        disToGround2 = 0;

        bool onIce = false;

        //右脚线
        RaycastHit hit1;
        Vector3 pos1 = transform.position + new Vector3(0.2f, 0f, 0);
        //检测
        if (showLine)
        {
            Debug.DrawRay(pos1, Vector3.down * rayLen, Color.red);
        }
        isFootHit1 = Physics.Raycast(pos1, Vector3.down, out hit1, rayLen, layerCollision);
        float hitDis1 = hit1.distance;
        if (Mathf.Abs(hit1.distance - originPoint2Ground) < 0.0001)
        {
            hitDis1 = originPoint2Ground;
        }
        disToGround1 = hitDis1 - originPoint2Ground;
        angleToGround1 = VectorAngle(hit1.normal, Vector3.up);
        if (isFootHit1 && "IceGround".Equals(hit1.transform.tag))
        {
            onIce = true;
        }

        //左脚线
        RaycastHit hit2;
        Vector3 pos2 = transform.position + new Vector3(-0.2f, 0f, 0);
        //检测
        if (showLine)
        {
            Debug.DrawRay(pos2, Vector3.down * rayLen, Color.red);
        }
        isFootHit2 = Physics.Raycast(pos2, Vector3.down, out hit2, rayLen, layerCollision);
        float hitDis2 = hit2.distance;
        if (Mathf.Abs(hit2.distance - originPoint2Ground) < 0.0001)
        {
            hitDis2 = originPoint2Ground;
        }
        disToGround2 = hitDis2 - originPoint2Ground;
        angleToGround2 = VectorAngle(hit2.normal, Vector3.up);
        if (isFootHit2 && "IceGround".Equals(hit2.transform.tag))
        {
            onIce = true;
        }

        //中间线
        RaycastHit hit;
        Vector3 pos = transform.position + new Vector3(0f, 0f, 0);
        //检测
        if (showLine)
        {
            Debug.DrawRay(pos, Vector3.down * rayLen, Color.red);
        }
        isFootHit = Physics.Raycast(pos, Vector3.down, out hit, rayLen, layerCollision);
        float hitDis = hit.distance;
        if (Mathf.Abs(hit.distance - originPoint2Ground) < 0.0001)
        {
            hitDis = originPoint2Ground;
        }
        //与地面距离
        disToGround = hitDis - originPoint2Ground;
        angleToGround = VectorAngle(hit.normal, Vector3.up);

        if (isFootHit && "IceGround".Equals(hit.transform.tag))
        {
            onIce = true;
        }

        hasHitIceGround = onIce;

        midNormal = Vector3.zero;

        Vector3 normal1 = hit.normal;
        Vector3 normal2 = hit1.normal;
        Vector3 normal3 = hit2.normal;

        //边线未命中，则设为朝上。
        if (isFootHit == false)
        {
            normal1 = Vector3.zero;
        }
        if (isFootHit1 == false)
        {
            normal2 = Vector3.zero;
        }
        if (isFootHit2 == false)
        {
            normal3 = Vector3.zero;
        }

        midNormal = (normal1 + normal2 + normal3).normalized;

        if (isFootHit && isFootHit1 && midNormal.x < 0)
        {
            normal3 = Vector3.zero;
        }
        if (isFootHit && isFootHit2 && midNormal.x > 0)
        {
            normal2 = Vector3.zero;
        }
        //重算midNormal。
        midNormal = (normal1 + normal2 + normal3).normalized;
        setCheckedSlope();

        //		float a = VectorAngle (midNormal,Vector3.up);
        //		print ("midNormal angle = "+a);

        if (showLine)
        {
            Debug.DrawRay(transform.position, midNormal * 2, Color.blue);
        }

        CheckOnGround();
    }

    void CheckOnGround()
    {
        //命中地面且距离为0，表示紧贴地面。否则表示在空中。在空中又分为上升、下落、跌落。

        //设置是否在地面
        if ((isFootHit && disToGround == 0) || (isFootHit1 && disToGround1 == 0) || (isFootHit2 && disToGround2 == 0))
        {
            state = BoyState.Grounded;
        }
        else
        {
            //print("从高台跌落");
            //在空中的情况下，如果当前状态是grounded，说明这一帧从高台跌落。
            if (state == BoyState.Grounded)
            {

                state = BoyState.Falling;
                //跌落速度。高空跌落时，初始速度为0。当低空且贴近地面跌落时，初始下落速度设为较大值。                    
                fallSpeed = 5;
                //fallSpeed = 0;

            }
        }
    }

    //计算检测到的斜率
    void setCheckedSlope()
    {
        float angle = Vector3.Angle(midNormal, Vector3.up);
        if (midNormal == Vector3.zero)
        {
            angle = 0;
        }
        slope = angle / 90;
    }

}
