using UnityEngine;
using System.Collections;

//BoyAI有三种跟随方法：1.跟随点（走到指定位置点）；2.跟随对象；3.按向量移动。
//两种跟随策略：1.直接跟随。2.按导航跟随。
//需要停止跟随，并不再响应跟随指令，调用DisableFollow()方法或设置enableFollow = false即可，推荐前者。
//需要停止跟随，但可以响应跟随指令，调用StayThere()即可。
public class BoyAI : MonoBehaviour, GameManagerRoleListener
{
    public PlayerControl cat;

    public BoyController boy;

    public DangerDetector dangerDct;

    public JumpDetector jmpDct;

    public NavManage nav;

    public FollowPointsManage pointsManage;

    ////能否安全移动
    //public bool movable = true;

    //启用跟随
    public bool enableFollow = true;

    //开启导航
    public bool enableNav;

    //跟随模式
    public FollowMode mode;
    public bool arrived = true;
    public float speed;

    //策略
    public IFollowStrategy followStrategy;
    public FollowNav followNav;
    public FollowDirect followDirect;

    //跟随点
    public Vector3 targetPoint;
    //跟随目标
    public Transform targetTransfrom;
    //按向量移动
    public Vector3 vec;


    //输出小男孩的水平速度，仅观察用.
    public float horSpeed = 1;

    public float minDis = 2;

    public float slowDis = 5;


    //跟随点测试开启
    public bool enableFollowPointTest = false;

    // Use this for initialization
    void Start()
    {
        cat = GameManager.Instance.Player;
        boy = transform.GetComponent<BoyController>();
        dangerDct = transform.GetComponent<DangerDetector>();
        jmpDct = transform.Find("JumpDetector").GetComponent<JumpDetector>();

        followNav = new FollowNav(boy, nav);
        followDirect = new FollowDirect(boy);

        GameManager.Instance.AddRoleListener(this);

        //初始化
        mode = FollowMode.FollowTarget;
        targetTransfrom = cat.transform;
    }

    // Update is called once per frame
    void Update()
    {
        //无论是否用AI，都要调用DangerDct
        dangerDct.UpdateByParent();

        if (boy.UseAI)
        {
            //召唤指令
            if (GameManager.Instance.playerIsDead == false && boy.roleActionsControl.Player_RightTrigger.IsPressed)
            {
                FollowTarget(cat.gameObject);
            }

            //处理跟随
            if (enableFollow)
            {
                Processing();
            }

            //enableFollow==false时小男孩停止
            if (enableFollow == false)
            {
                StayThere();
            }
        }


        if (enableFollowPointTest)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                GotoPoint(boy.transform.position + Vector3.right * 10);
            }
        }

    }

    //每帧处理
    void Processing()
    {
        //是否开启导航
        if (nav.inNavmesh)
        {
            followStrategy = followNav;
        }
        else
        {
            followStrategy = followDirect;
        }

        //可着力的情况下，小男孩能走，能停。
        if ((boy.groundCheck.IsOnGround() || boy.isFloating))
        {
            //获取目标位置
            Vector3 pos;
            if (mode == FollowMode.FollowPoint)
            {
                pos = targetPoint;
            }
            else if (mode == FollowMode.FollowTarget)
            {
                pos = targetTransfrom.position;
            }
            else
            {
                return;
            }

            //初始化目标位置
            followStrategy.InitTargetPostion(pos);

            //是否到达
            if (arrived == false)
            {
                //调整朝向（调整朝向后，有2个选择：1.能移动则移动；2.不能移动则跳）
                AdjustFacing();

                //2.能移动
                if (IsBoyMovable() && (dangerDct.passable || boy.isFloating))
                {
                    //设置速度
                    SetSpeed();
                    //跟随。
                    followStrategy.Follow();

                    //是否抵达
                    arrived = followStrategy.IsArrived();
                }
                else
                {
                    //3.不能移动，跳检测.成功则跳。失败则设置已到达。（刚进力场时，boy.groundCheck.IsOnGround()经常为true，导致boy不跟随，垂直下落）
                    if (boy.groundCheck.IsOnGround() && boy.isFloating == false && boy.moveProc.CanMoveFreely)
                    {
                        //print("尝试跳" + boy.groundCheck.IsOnGround());
                        TryJump();
                    }
                }
            }
            //follow执行完成之后，可能会变成arrived==true;所以不能使用else.
            if (arrived)
            {
                StayThere();
                LookAtTarget();
            }
        }
    }

    //走到指定点
    public void GotoPoint(Vector3 point)
    {
        arrived = false;
        mode = FollowMode.FollowPoint;
        targetPoint = point;
    }

    //跟随目标
    public void FollowTarget(GameObject target)
    {
        arrived = false;
        mode = FollowMode.FollowTarget;
        targetTransfrom = target.transform;
    }

    //按向量移动（长度为向量长度）
    public void MoveVector(Vector3 vec)
    {
        Vector3 pos = boy.transform.position + vec;
        GotoPoint(pos);
    }

    //站定（停下，可以响应跟随指令）
    public void StayThere()
    {
        arrived = true;
        boy.moveProc.SetMoveByAI(0);
        this.horSpeed = 0;
    }

    //2秒后看向猫
    public void LookAtTarget()
    {
        //跟随对象的情况下，才面朝对象
        if (mode == FollowMode.FollowTarget)
        {
            StartCoroutine(_LookAtTarget());
        }
    }

    //延迟面向猫
    private IEnumerator _LookAtTarget()
    {
        yield return new WaitForSeconds(2);

        //跟随对象的情况下，才面朝对象
        if (mode == FollowMode.FollowTarget)
        {
            //静止时，自动面朝猫（1.5内不转身）
            float disHor2 = Mathf.Abs(boy.transform.position.x - cat.transform.position.x);
            if (disHor2 > 1.5f)
            {
                if (boy.transform.position.x > cat.transform.position.x)
                {
                    boy.moveProc.TurnByOrder(4);
                }
                else
                {
                    boy.moveProc.TurnByOrder(6);
                }
            }
        }
    }

    //禁用跟随（不再响应跟随指令）
    public void DisableFollow()
    {
        enableFollow = false;
    }

    //设置速度
    void SetSpeed()
    {
        speed = 1;
        //计算速度
        float remainDis = followStrategy.RemainDistance();
        if (remainDis < slowDis)
        {
            //speed = remainDis / slowDis;
            speed = (remainDis - minDis) / (slowDis - minDis);
            if (speed < 0.3)
            {
                speed = 0.3f;
            }
        }

        if (followStrategy.IsToRight() == false)
        {
            speed = -Mathf.Abs(speed);
        }
        this.horSpeed = speed;
    }

    //面朝前进方向。
    bool AdjustFacing()
    {
        //方向是否一致
        if (boy.moveProc.faceLeft != followStrategy.IsToRight())
        {
            return true;
        }
        else
        {
            if (followStrategy.IsToRight())
            {
                boy.moveProc.TurnByOrder(6);
            }
            else
            {
                boy.moveProc.TurnByOrder(4);
            }
            return false;
        }
    }

    bool IsBoyMovable()
    {
        if (followStrategy.IsToRight() && boy.moveProc.moveToRight)
        {
            return true;
        }
        else if (followStrategy.IsToRight() == false && boy.moveProc.moveToLeft)
        {
            return true;
        }
        return false;
    }

    void TryJump()
    {
        jmpDct.Detect();
        if (jmpDct.jumpable)
        {
            boy.jumpProc.JumpByAI(jmpDct.isClimbWallTrigger);
            boy.moveProc.SetMoveByAI(jmpDct.bestSpeed);
            this.horSpeed = jmpDct.bestSpeed;
        }
        else
        {
            //不能执行的命令，取消掉。（防止切换世界之后，跑到在原来世界定的坐标点）
            arrived = true;
        }
    }

    public void PlayerDead()
    {
        //print("死亡");
        //throw new System.NotImplementedException();
    }

    public void PlayerRespawn()
    {
        StayThere();
    }
}

//跟随方式
public enum FollowMode
{
    //NoFollow,
    FollowTarget,
    FollowPoint
}




