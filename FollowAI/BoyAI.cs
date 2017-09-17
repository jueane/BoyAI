using UnityEngine;
using System.Collections;

public class BoyAI : MonoBehaviour, GameManagerRoleListener
{
    public PlayerControl cat;

    public BoyController boy;

    public DangerDetector pathDct;

    JumpDetector jmpDct;

    ////能否安全移动
    //public bool movable = true;

    //启用跟随
    public bool enableFollow = true;
    //启用设置目标功能
    public bool enableSetTarget = true;

    public FollowMode mode;
    public IFollowStrategy followStrategy;
    public bool arrived = true;
    public Vector3 posTarget;

    //输出小男孩的水平速度，仅观察用.
    public float horSpeed = 1.5f;

    public float minDis = 2;


    // Use this for initialization
    void Start()
    {
        cat = GameManager.Instance.Player;
        boy = transform.GetComponent<BoyController>();
        pathDct = transform.GetComponent<DangerDetector>();
        jmpDct = transform.Find("JumpDetector").GetComponent<JumpDetector>();

        posTarget = Vector3.zero;

        GameManager.Instance.AddRoleListener(this);
    }

    // Update is called once per frame
    void Update()
    {
        //无论是否用AI，都要调用DangerDct
        pathDct.UpdateByParent();

        if (boy.UseAI && enableFollow)
        {
            Process();
        }

        //enableFollow==false时小男孩停止
        if (boy.UseAI && enableFollow == false)
        {
            //print("停止");
            boy.moveProc.SetMoveByAI(0);
            horSpeed = 0;
            posTarget = Vector3.zero;
        }

    }

    void Process()
    {
        if (enableSetTarget && boy.roleActionsControl.Player_RightTrigger.IsPressed && GameManager.Instance.playerIsDead == false)
        {
            arrived = false;
            if (mode == FollowMode.FollowCat)
            {
                followStrategy = new FollowCat(cat, boy);
            }
            else if (mode == FollowMode.FollowNav)
            {
                followStrategy = new FollowNav(cat, boy);
            }
            else
            {
                followStrategy = new FollowPoint(cat, boy);
            }

            ////手动切换跟随模式

            //if (mode == FollowMode.FollowCat)
            //{
            //    //目标是猫
            //    arrived = false;
            //}
            //else
            //{
            //    //目标是点                
            //    arrived = false;
            //    posTarget = GameManager.Instance._cameraController.transform.position;
            //}

        }

        ////跟随猫模式
        //if (mode == FollowMode.FollowCat && arrived == false)
        //{
        //    float disHor2 = Mathf.Abs(boy.transform.position.x - cat.transform.position.x);
        //    if (disHor2 < minDis)
        //    {
        //        arrived = true;
        //    }
        //    else
        //    {
        //        posTarget = cat.transform.position;
        //    }
        //}

        ////跟随点模式
        //if (mode == FollowMode.FollowPoint && arrived == false)
        //{
        //    float disHor = Mathf.Abs(boy.transform.position.x - posTarget.x);
        //    if (disHor < minDis)
        //    {
        //        arrived = true;
        //    }
        //}

        //可着力的情况下，小男孩能走，能停。
        if ((boy.groundCheck.IsOnGround() || boy.isFloating))
        {
            //是否到达
            if (arrived == false)
            {
                //1.转身
                //followStrategy.LookatCat();
                AdjustDirection();

                //2.能移动
                if (IsBoyMovable())
                {
                    //Follow();
                    followStrategy.Follow();
                }
                else
                {
                    //3.不能移动，跳检测.成功则跳。失败则设置已到达。
                    TryJump();
                }

            }
            //follow执行完成之后，可能会变成arrived==true;所以不能使用else.
            if (arrived)
            {
                StayThere();
            }
        }
    }

    void Follow()
    {
        if (AdjustDirection())
        {
            //print("步骤7" + "," + IsBoyMovable() + "," + pathDct.passable + "," + boy.isFloating);
            if (IsBoyMovable() && (pathDct.passable || boy.isFloating))
            {
                //print("步骤8");
                float speedTemp = 1;
                //if (pathDct.isDanger)
                //{
                //    speedTemp = 0.3f;
                //}

                if (IsFollowingRight())
                {
                    boy.moveProc.SetMoveByAI(speedTemp);
                    this.horSpeed = speedTemp;
                }
                else
                {
                    boy.moveProc.SetMoveByAI(-speedTemp);
                    this.horSpeed = -speedTemp;
                }
            }
            else
            {
                //不能移动，先停下，尝试跳过。还要判断是否僵直canMoveFreely.
                if ((boy.groundCheck.IsOnGround() || boy.isFloating) && boy.moveProc.CanMoveFreely)
                {
                    //先停下
                    boy.moveProc.SetMoveByAI(0);
                    this.horSpeed = 0;
                    //再尝试跳
                    TryJump();
                }
            }
        }
    }

    public void StayThere()
    {
        posTarget = Vector3.zero;
        boy.moveProc.SetMoveByAI(0);
        this.horSpeed = 0;

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
            //posTarget = Vector3.zero;
            arrived = true;
        }
    }

    public bool IsFollowingRight()
    {
        //if (boy.transform.position.x <= GameManager.Instance.Player.transform.position.x)
        if (boy.transform.position.x <= posTarget.x)
        {
            return true;
        }
        return false;
    }

    public void PlayerDead()
    {
        //print("死亡");
        //throw new System.NotImplementedException();
    }

    public void PlayerRespawn()
    {
        posTarget = Vector3.zero;
    }
}


public enum FollowMode
{
    FollowCat,
    FollowNav,
    FollowPoint
}




