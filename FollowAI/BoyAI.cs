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
        //在力场中，跟随距离为0.5;
        //if (boy.isFloating)
        //{
        //    minDis = 0.5f;
        //}
        //else
        //{
        //    minDis = 2;
        //}

        if (enableSetTarget && boy.roleActionsControl.Player_RightTrigger.IsPressed && GameManager.Instance.playerIsDead == false)
        {
            //手动切换跟随模式

            if (mode == FollowMode.FollowCat)
            {
                //目标是猫
                arrived = false;
            }
            else
            {
                //目标是点                
                arrived = false;
                posTarget = GameManager.Instance._cameraController.transform.position;
            }



            //自动切换跟随模式

            ////检查相机是否归位（要算上镜头偏移)
            //Vector2 p1 = GameManager.Instance._cameraController.CameraOffset;
            //Vector2 p2 = GameManager.Instance._cameraController._lastTargetPosition;
            //Vector2 p3 = GameManager.Instance._cameraController._lastCameraPosition;

            ////相机是否归位（前提是猫没有移动）
            //bool cameraIsClosed = cat.moveProc.horizontalInputSpeed == 0 && Vector3.Distance(p1 + p2, p3) < 7f;

            ////print("设置目标");
            //if (cameraIsClosed)
            //{
            //    //目标是猫
            //    mode = FollowMode.FollowCat;
            //    arrived = false;
            //}
            //else
            //{
            //    //目标是点
            //    mode = FollowMode.FollowPoint;
            //    arrived = false;
            //    posTarget = GameManager.Instance._cameraController.transform.position;
            //}

        }

        //跟随猫模式
        if (mode == FollowMode.FollowCat && arrived == false)
        {
            float disHor2 = Mathf.Abs(boy.transform.position.x - cat.transform.position.x);
            if (disHor2 < minDis)
            {
                arrived = true;
            }
            else
            {
                posTarget = cat.transform.position;
            }
        }

        //跟随点模式
        if (mode == FollowMode.FollowPoint && arrived == false)
        {
            float disHor = Mathf.Abs(boy.transform.position.x - posTarget.x);
            if (disHor < minDis)
            {
                arrived = true;
            }
        }

        //可着力的情况下才能改变小男孩的移动状态
        if ((boy.groundCheck.IsOnGround() || boy.isFloating))
        {
            //是否到达
            if (arrived == false)
            {
                Follow();
            }
            else
            {
                posTarget = Vector3.zero;
                boy.moveProc.SetMoveByAI(0);
                this.horSpeed = 0;

                //静止时，自动面朝猫
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

    //判断方向正确。
    bool AdjustDirection()
    {
        //方向是否一致
        if (boy.moveProc.faceLeft != IsFollowingRight())
        {
            return true;
        }
        else
        {
            //方向不同，则先转身。
            //boy.SetMoveByAI(0);
            //this.horSpeed = 0;
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




