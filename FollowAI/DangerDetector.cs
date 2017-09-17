using UnityEngine;
using System.Collections;

public class DangerDetector : MonoBehaviour
{
    BoyAI ai;
    int interestLayer;

    public Collider[] cArr;
    public float passHeight;
    public bool existPath;
    public bool isDanger = false;
    public float slope;
    public bool passable;
    public bool isFloating;
    public Vector3 normal;
    public GameObject hitObj;

    // Use this for initialization
    void Start()
    {
        ai = GameManager.Instance.boy.GetComponent<BoyAI>();
        interestLayer = LayerMask.GetMask("ground", "Platform", "Danger", "Floating");
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateByParent()
    {
        //如果在空中，则passable为true.
        if (ai.boy.groundCheck.IsOnGround() == false)
        {
            //初始化
            this.normal = Vector3.up;
            this.hitObj = null;
            this.slope = 0;
            this.isDanger = false;
            this.passable = true;

            return;
        }

        Reset();

        DetectExistPath();

        if (existPath)
        {
            if (passable == false || ai.boy.isFloating)
            {
                DetectStandableNear();
            }
        }
        else
        {
            passHeight = -1;
        }
    }

    //纵向扫描身前是否存在位置
    void DetectExistPath()
    {
        Vector3 pos = transform.position + Vector3.up;
        //设置检测方向的依据。
        if (ai.boy.moveProc.faceLeft)
        {
            pos -= new Vector3(1.2f, 0, 0);
        }
        else
        {
            pos += new Vector3(1.2f, 0, 0);
        }

        //检测范围稍大点，可以避免离danger过近。
        Vector3 size = new Vector3(0.6f, 2, 0);

        existPath = false;
        for (int i = 0; i < 10; i++)
        {
            passHeight = i * 0.4f + 0.01f;
            cArr = Physics.OverlapBox(pos + new Vector3(0, passHeight, 0), size / 2, Quaternion.identity, interestLayer, QueryTriggerInteraction.Collide);

            if (cArr.Length == 0)
            {
                existPath = true;
                break;
            }
            else
            {
                for (int j = 0; j < cArr.Length; j++)
                {
                    if (LayerMask.NameToLayer("Floating").Equals(cArr[j].gameObject.layer))
                    {
                        passHeight = 0;
                        existPath = true;
                        isDanger = false;
                        slope = 0;
                        passable = true;
                        return;
                    }
                }

            }


        }

    }

    //如果有，检测身前是否危险、可站立（近距离）
    void DetectStandableNear()
    {
        Vector3 pos = transform.position + Vector3.up;
        //设置检测方向的依据。
        if (ai.boy.moveProc.faceLeft)
        {
            pos -= new Vector3(1f, 0, 0);
        }
        else
        {
            pos += new Vector3(1f, 0, 0);
        }
        Vector3 originPoint = pos + Vector3.up * passHeight;

        ////向下打线，取命中法线斜度。
        RaycastHit hit;

        bool isHit = Physics.Raycast(originPoint, Vector3.down, out hit, 20, interestLayer);

        Debug.DrawRay(originPoint, Vector3.down * 20, Color.blue);

        if (isHit)
        {
            this.hitObj = hit.collider.gameObject;
            this.normal = hit.normal;
            this.slope = GetSlope(hit.normal);
            if (hit.collider && hit.collider.gameObject.layer == LayerMask.NameToLayer("Danger"))
            {
                isDanger = true;
            }
            else
            {
                isDanger = false;
            }


        }


        if (isHit && slope < 0.5f && isDanger == false)
        {
            passable = true;
        }
        else if (isHit && slope >= 0.5f && isDanger == false)
        {
            DetectStandableFar();
        }
        else
        {
            passable = false;
        }

    }

    //如果有，检测身前是否危险、可站立（稍远）
    void DetectStandableFar()
    {
        Vector3 pos = transform.position + Vector3.up;
        //设置检测方向的依据。
        if (ai.boy.moveProc.faceLeft)
        {
            pos -= new Vector3(0.75f, 0, 0);
        }
        else
        {
            pos += new Vector3(0.75f, 0, 0);
        }
        Vector3 originPoint = pos + Vector3.up * passHeight;

        //【这个else if可以选择一定数量的循环，以应对下滑的安全大斜面】
        //如果命中且不危险，则打第二条线，来断定前方是否很小的上下斜坡。
        //需要判断两次命中的位置差

        RaycastHit hit2;

        bool isHit2 = Physics.Raycast(originPoint + new Vector3(0.5f, 0, 0), Vector3.down, out hit2, 20, interestLayer);

        Debug.DrawRay(originPoint + new Vector3(0.5f, 0, 0), Vector3.down * 20, Color.blue);

        if (isHit2)
        {
            this.hitObj = hit2.collider.gameObject;
            this.normal = hit2.normal;
            this.slope = GetSlope(hit2.normal);
            if (hit2.collider && hit2.collider.gameObject.layer == LayerMask.NameToLayer("Danger"))
            {
                isDanger = true;
            }
            else
            {
                isDanger = false;
            }
            if (slope < 0.5f && isDanger == false)
            {
                passable = true;
            }
            else
            {
                passable = false;
            }

        }

    }

    void Reset()
    {
        //初始化
        this.normal = Vector3.zero;
        this.hitObj = null;
        this.slope = 0;
        this.isDanger = true;
        this.passable = false;
    }

    //判断是否在力场中。
    //public bool IsInFloatingArea()
    //{
    //    float offset = 0;
    //    if (ai.IsFollowingRight())
    //    {
    //        offset = 0.8f;
    //    }
    //    else
    //    {
    //        offset = -0.8f;
    //    }

    //    bool isA = Physics.CheckBox(transform.position + Vector3.up, new Vector3(1, 2, 1), Quaternion.identity, LayerMask.GetMask("Floating"));
    //    bool isB = Physics.CheckBox(transform.position + Vector3.up + new Vector3(offset, 0, 0), new Vector3(1, 2, 1), Quaternion.identity, LayerMask.GetMask("Floating"));
    //    isFloating = isA | isB;
    //    return isA || isB;
    //}

    //计算检测到的斜率
    float GetSlope(Vector3 normal)
    {
        float angle = Vector3.Angle(normal, Vector3.up);
        if (normal == Vector3.zero)
        {
            angle = 0;
        }
        return angle / 90;
    }

}
