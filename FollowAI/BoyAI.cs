using UnityEngine;
using System.Collections;

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
    //启用设置目标功能
    public bool enableSetTarget = true;

    //策略模式实现
    public FollowMode mode;
    public IFollowStrategy followStrategy;
    private FollowCat followCat;
    private FollowNav followNav;
    public FollowPoint followPoint;
    public bool arrived = true;
    public float speed;

    //输出小男孩的水平速度，仅观察用.
    public float horSpeed = 1;

    public float minDis = 2;

    public float slowDis = 5;

    // Use this for initialization
    void Start()
    {
        cat = GameManager.Instance.Player;
        boy = transform.GetComponent<BoyController>();
        dangerDct = transform.GetComponent<DangerDetector>();
        jmpDct = transform.Find("JumpDetector").GetComponent<JumpDetector>();

        followCat = new FollowCat(cat, boy);
        followNav = new FollowNav(cat, boy, nav);
        followPoint = new FollowPoint(cat, boy);

        GameManager.Instance.AddRoleListener(this);
    }

    // Update is called once per frame
    void Update()
    {
        //无论是否用AI，都要调用DangerDct
        dangerDct.UpdateByParent();

        if (boy.UseAI && enableFollow)
        {
            Process();
        }

        //enableFollow==false时小男孩停止
        if (boy.UseAI && enableFollow == false)
        {
            StayThere();
        }

    }

    void Process()
    {
        if (nav.inNavmesh)
        {
            mode = FollowMode.FollowNav;
        }
        else if (pointsManage.inPointArea)
        {
            mode = FollowMode.FollowPoint;
        }
        else
        {
            mode = FollowMode.FollowCat;
        }

        if (enableSetTarget && boy.roleActionsControl.Player_RightTrigger.IsPressed && GameManager.Instance.playerIsDead == false)
        {
            arrived = false;
        }
        switch (mode)
        {
            case FollowMode.FollowCat:
                followStrategy = followCat;
                break;

            case FollowMode.FollowNav:
                followStrategy = followNav;
                break;

            case FollowMode.FollowPoint:
                followStrategy = followPoint;
                break;
        }

        //可着力的情况下，小男孩能走，能停。
        if ((boy.groundCheck.IsOnGround() || boy.isFloating))
        {
            //是否到达
            if (arrived == false)
            {
                //1.转身
                AdjustDirection();

                //2.能移动
                if (IsBoyMovable() && (dangerDct.passable || boy.isFloating))
                {
                    //设置速度
                    SetSpeed();
                    //跟随。
                    followStrategy.Follow();
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
                StayThereAndLookAtCat();
            }
        }
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

        if (IsFollowingRight() == false)
        {
            speed = -Mathf.Abs(speed);
        }
        this.horSpeed = speed;
    }

    public void GotoPoint(Vector3 point)
    {
        followPoint.posTarget = point;
        arrived = false;
    }

    public void StayThere()
    {
        arrived = true;
        boy.moveProc.SetMoveByAI(0);
        this.horSpeed = 0;
    }

    public void StayThereAndLookAtCat()
    {
        StayThere();
        if (mode == FollowMode.FollowCat)
        {
            StartCoroutine(LookAtCat());
        }
    }

    IEnumerator LookAtCat()
    {
        yield return new WaitForSeconds(2);
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

    //面朝前进方向。
    bool AdjustDirection()
    {
        //方向是否一致
        if (boy.moveProc.faceLeft != IsFollowingRight())
        {
            return true;
        }
        else
        {
            if (IsFollowingRight())
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
        if (IsFollowingRight() && boy.moveProc.moveToRight)
        {
            return true;
        }
        else if (IsFollowingRight() == false && boy.moveProc.moveToLeft)
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
            boy.jumpProc.JumpByAI();
            boy.moveProc.SetMoveByAI(jmpDct.bestSpeed);
            this.horSpeed = jmpDct.bestSpeed;
        }
        else
        {
            //不能执行的命令，取消掉。（防止切换世界之后，跑到在原来世界定的坐标点）
            arrived = true;
        }
    }

    public bool IsFollowingRight()
    {
        return followStrategy.IsToRight();
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


public enum FollowMode
{
    FollowCat,
    FollowNav,
    FollowPoint
}




