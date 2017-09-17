using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JumpDetector : MonoBehaviour
{
    //计算检测间隔
    float interval = 0;
    public bool enableInterval = false;

    //全部检测体
    public JumpDetectorBody[] dctBodyArr = new JumpDetectorBody[5];
    //生还的检测体
    public List<JumpDetectorBody> dctBodyList = new List<JumpDetectorBody>();

    BoyAI ai;

    //母体
    JumpDetectorBody jd;

    //临时存放对象
    Transform tempMother;

    //生还数量
    public int surviveCount;
    public bool jumpable;
    public float bestSpeed;

    //检测体自动编号
    int number = 0;

    // Use this for initialization
    void Start()
    {
        ai = GameManager.Instance.boy.GetComponent<BoyAI>();
        jd = transform.parent.Find("JumpDetectorBody").GetComponent<JumpDetectorBody>();
        tempMother = GameManager.Instance.temp.transform;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKey(KeyCode.J))
        //{
        //    Detect();
        //}
    }

    //执行一次检测（外部调用）
    public void Detect()
    {
        //检测前必须重置参数，否则会出现使用上一次检测结果的情况。
        jumpable = false;
        surviveCount = 0;
        bestSpeed = 0;

        //每次检测都要间隔一段时间，避免每帧消耗大量资源。
        if (enableInterval)
        {
            interval += Time.deltaTime;
            if (interval < 0.5f)
            {
                return;
            }
            else
            {
                interval = 0;
            }
        }

        //清理之前创建的检测体（放在这个位置清除，可以保证不会遗漏）
        Clean();

        CreateAndDetect();

        ProcessResult();
    }

    //清理检测体
    void Clean()
    {
        for (int i = 0; i < dctBodyArr.Length; i++)
        {
            if (dctBodyArr[i])
            {
                Destroy(dctBodyArr[i].gameObject);
            }
            dctBodyArr[i] = null;
        }

    }

    //创建检测体并执行检测
    void CreateAndDetect()
    {
        //创建新的检测体

        for (int i = 0; i < dctBodyArr.Length; i++)
        {
            JumpDetectorBody jdTemp = Instantiate<JumpDetectorBody>(jd);
            jdTemp.name = "jd" + number++;
            jdTemp.transform.position = ai.boy.transform.position + Vector3.up;
            jdTemp.transform.SetParent(this.tempMother);

            jdTemp.Init();

            float speedTmp = 1 - (1f / dctBodyArr.Length) * (i);

            if (ai.IsFollowingRight())
            {
                jdTemp.usedSpeed = speedTmp;
                jdTemp.Jump(speedTmp);
            }
            else
            {
                jdTemp.usedSpeed = -speedTmp;
                jdTemp.Jump(-speedTmp);
            }

            dctBodyArr[i] = jdTemp;
        }

    }

    //处理检测结果
    void ProcessResult()
    {
        //碰撞完成的检测体数量
        int collidedCount = 0;
        dctBodyList.Clear();
        for (int i = 0; i < dctBodyArr.Length; i++)
        {
            //发生碰撞的数量
            if (dctBodyArr[i] && dctBodyArr[i].isCollided)
            {
                collidedCount++;
                //碰撞且生还的数量
                if (dctBodyArr[i].isSurvive)
                {
                    dctBodyList.Add(dctBodyArr[i]);
                }
            }

        }

        //生还数量
        int count = dctBodyList.Count;
        surviveCount = dctBodyList.Count;
        if (count > 0)
        {
            //找出最优跳跃速度
            //bestSpeed = dctBodyList[(dctBodyList.Count-1)/2].usedSpeed;
            bestSpeed = dctBodyList[0].usedSpeed;


            jumpable = true;
        }
        else
        {
            jumpable = false;
        }

    }
}

