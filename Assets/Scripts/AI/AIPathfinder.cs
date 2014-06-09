using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AIPathfinder : MonoBehaviour
{
    float m_Run = 0.0f;

    LinkedList<AIDataNode> m_DataNodes;
    LinkedList<AIDataNode> m_ClosedList;
    AIDataNode m_Path;

    public GameObject m_Target = null;
    public GameObject m_Unit = null;

    public float m_BaseMove = 0.1f;
    public float m_BaseRotate = 18.0f * Mathf.Deg2Rad * 0.5f;
    public float m_ColliderHeight = 0.5f;
    public float m_ColliderRadius = 0.5f;

    // Use this for initialization
    void Start()
    {
        m_DataNodes = new LinkedList<AIDataNode>();
        m_ClosedList = new LinkedList<AIDataNode>();
    }

    public void setRunFromMovement()
    {
        UnitMover unitMover = m_Unit.GetComponentInChildren<UnitMover>();
        m_Run = unitMover.parentUnit.maximumMovement;
    }

    bool AddForward(Vector3 pos, Vector3 forward, float halfwidth)
    {
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 colliderOffset = new Vector3(0.0f, m_ColliderHeight) + forward * 0.5f;
        Vector3 tl = pos - right * halfwidth + colliderOffset;
        Vector3 tr = pos + right * halfwidth + colliderOffset;

        bool add = true;
        RaycastHit hitObj;
        if (Physics.CheckSphere(tl, m_ColliderRadius))
        {
            add = false;
        }
        if (Physics.CheckSphere(tr, m_ColliderRadius))
        {
            add = false;
        }
        if (Physics.SphereCast(tr, m_ColliderRadius, tl - tr, out hitObj, halfwidth * 2.0f))
        {
            Debug.Log(hitObj.transform.tag + " hit at count of " + m_DataNodes.Count.ToString());
            if (hitObj.transform.tag == "Obstacle")
            {
                add = false;
            }
        }
        if(Physics.SphereCast(tl,m_ColliderRadius,tr-tl,out hitObj, halfwidth*2.0f))
        {
            Debug.Log(hitObj.transform.tag + " hit at count of " + m_DataNodes.Count.ToString());
            if (hitObj.transform.tag == "Obstacle")
            {
                add = false;
            }
        }

        return add;
    }

    bool AddCounterClockwise(Vector3 pos, Vector3 right, float halfwidth, float curAngle, out Vector3 newPos, out Vector3 newForward, out float nextAngle)
    {
        float current = curAngle * Mathf.Deg2Rad;
        nextAngle = current + m_BaseRotate;

        Vector3 newRight = (new Vector3(Mathf.Cos(nextAngle), 0.0f, Mathf.Sin(nextAngle))).normalized;
        newForward = Vector3.Cross(newRight, Vector3.up).normalized;

        Vector3 posChange = (pos - right * halfwidth) - (pos - newRight * halfwidth);

        newPos = pos + posChange;

        Vector3 colliderOffset = new Vector3(0.0f, m_ColliderHeight) + newForward * 0.5f;
        Vector3 tl = newPos - newRight * halfwidth + colliderOffset;
        Vector3 tr = newPos + newRight * halfwidth + colliderOffset;

        bool add = true;
        RaycastHit hitObj;
        if (Physics.CheckSphere(tl, m_ColliderRadius))
        {
            add = false;
        }
        if (Physics.CheckSphere(tr, m_ColliderRadius))
        {
            add = false;
        }
        if (Physics.SphereCast(tl, m_ColliderRadius, tr - tl, out hitObj, halfwidth * 2.0f))
        {
            Debug.Log(hitObj.transform.tag + " hit at count of " + m_DataNodes.Count.ToString());
            if (hitObj.transform.tag == "Obstacle")
            {
                add = false;
            }
        }

        return add;
    }
    bool AddClockwise(Vector3 pos, Vector3 right, float halfwidth, float curAngle, out Vector3 newPos, out Vector3 newForward, out float nextAngle)
    {
        float current = curAngle * Mathf.Deg2Rad;
        nextAngle = current - m_BaseRotate;

        Vector3 newRight = (new Vector3(Mathf.Cos(nextAngle), 0.0f, Mathf.Sin(nextAngle))).normalized;
        newForward = Vector3.Cross(newRight, Vector3.up).normalized;

        Vector3 posChange = (pos + right * halfwidth) - (pos + newRight * halfwidth);

        newPos = pos + posChange;

        Vector3 colliderOffset = new Vector3(0.0f, m_ColliderHeight) + newForward * 0.5f;
        Vector3 tl = newPos - newRight * halfwidth + colliderOffset;
        Vector3 tr = newPos + newRight * halfwidth + colliderOffset;

        bool add = true;
        RaycastHit hitObj;
        if (Physics.CheckSphere(tr, m_ColliderRadius))
        {
            add = false;
        }
        if (Physics.CheckSphere(tl, m_ColliderRadius))
        {
            add = false;
        }
        if (Physics.SphereCast(tr, m_ColliderRadius, tl - tr, out hitObj, halfwidth * 2.0f))
        {
            Debug.Log(hitObj.transform.tag + " hit at count of " + m_DataNodes.Count.ToString());
            if (hitObj.transform.tag == "Obstacle")
            {
                add = false;
            }
        }

        return add;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Run > 0.0f && m_Target != null && m_Unit != null)
        {
            // no nodes to analyze
            if(m_DataNodes.Count == 0)
            {
                UnitMover unitMover = m_Unit.GetComponentInChildren<UnitMover>();
                Vector3 unitForward = m_Unit.transform.forward;
                Vector3 unitSide = m_Unit.transform.right;
                float halfwidth = unitMover.parentUnit.Files * 0.5f;
                Vector3 unitPos = unitMover.parentUnit.transform.position;

                //attempt move forward

                if (AddForward(unitPos, unitForward, halfwidth))
                {
                    AIDataNode newNode = new AIDataNode();
                    newNode.m_MoveType = MoveType.MoveForward;
                    newNode.m_CostSoFar = m_BaseMove;
                    newNode.m_Parent = null;
                    newNode.m_Position = unitPos + unitForward * m_BaseMove;
                    newNode.m_Forward = unitForward;
                    newNode.m_MoveAmount = m_BaseMove;
                    newNode.m_CurrentAngle = unitMover.currentRotationAngle;
                    newNode.m_DistanceToGoal = Vector3.Distance(m_Target.transform.position, newNode.m_Position);
                    m_DataNodes.AddFirst(newNode);
                }

                //check if a turn CCW is possible
                float curAngle = unitMover.currentRotationAngle;

                Vector3 newPos, newForward;
                float nextAngle;
                if (AddCounterClockwise(unitPos, unitSide, halfwidth, curAngle, out newPos, out newForward, out nextAngle))
                {
                    AIDataNode newNode = new AIDataNode();
                    newNode.m_MoveType = MoveType.RotateCounterClockwise;
                    newNode.m_CostSoFar = (m_BaseRotate * Mathf.Rad2Deg) / 18.0f;
                    newNode.m_Parent = null;
                    newNode.m_Position = newPos;
                    newNode.m_Forward = newForward;
                    newNode.m_MoveAmount = m_BaseRotate * Mathf.Rad2Deg;
                    newNode.m_CurrentAngle = nextAngle;
                    newNode.m_DistanceToGoal = Vector3.Distance(m_Target.transform.position, newNode.m_Position);
                    m_DataNodes.AddFirst(newNode);
                }

                //check if a turn CW is possible
                if (AddClockwise(unitPos, unitSide, halfwidth, curAngle, out newPos, out newForward, out nextAngle))
                {
                    AIDataNode newNode = new AIDataNode();
                    newNode.m_MoveType = MoveType.RotateClockwise;
                    newNode.m_CostSoFar = (m_BaseRotate * Mathf.Rad2Deg) / 18.0f;
                    newNode.m_Parent = null;
                    newNode.m_Position = newPos;
                    newNode.m_Forward = newForward;
                    newNode.m_MoveAmount = m_BaseRotate * Mathf.Rad2Deg;
                    newNode.m_CurrentAngle = nextAngle;
                    newNode.m_DistanceToGoal = Vector3.Distance(m_Target.transform.position, newNode.m_Position);
                    m_DataNodes.AddFirst(newNode);
                }

                SortDataNodes();
                m_Run -= Time.deltaTime;
            }
            else
            {
                AIDataNode pathNode = m_DataNodes.First.Value;
                m_ClosedList.AddFirst(pathNode);
                m_DataNodes.RemoveFirst();


                if((pathNode.m_Position - m_Target.transform.position).magnitude < 1.0f)
                {
                    m_Run = 0.0f;
                }

                UnitMover unitMover = m_Unit.GetComponentInChildren<UnitMover>();
                Vector3 unitForward = pathNode.m_Forward;
                Vector3 unitSide = Vector3.Cross(Vector3.up, unitForward).normalized;
                float halfwidth = unitMover.parentUnit.Files * 0.5f;
                Vector3 unitPos = pathNode.m_Position;

                if (AddForward(unitPos, unitForward, halfwidth))
                {
                    AIDataNode newNode = new AIDataNode();
                    newNode.m_MoveType = MoveType.MoveForward;
                    newNode.m_CostSoFar = m_BaseMove;
                    newNode.m_Parent = pathNode;
                    newNode.m_Position = unitPos + unitForward * m_BaseMove;
                    newNode.m_Forward = unitForward;
                    newNode.m_MoveAmount = m_BaseMove;
                    newNode.m_DistanceToGoal = Vector3.Distance(m_Target.transform.position,newNode.m_Position);
                    m_DataNodes.AddFirst(newNode);
                }

                float curAngle = pathNode.m_CurrentAngle;

                Vector3 newPos, newForward;
                float nextAngle;
                if (AddCounterClockwise(unitPos, unitSide, halfwidth, curAngle, out newPos, out newForward, out nextAngle))
                {
                    AIDataNode newNode = new AIDataNode();
                    newNode.m_MoveType = MoveType.RotateCounterClockwise;
                    newNode.m_CostSoFar = (m_BaseRotate * Mathf.Rad2Deg) / 18.0f;
                    newNode.m_Parent = pathNode;
                    newNode.m_Position = newPos;
                    newNode.m_Forward = newForward;
                    newNode.m_MoveAmount = m_BaseRotate * Mathf.Rad2Deg;
                    newNode.m_CurrentAngle = nextAngle;
                    newNode.m_DistanceToGoal = Vector3.Distance(m_Target.transform.position, newNode.m_Position);
                    m_DataNodes.AddFirst(newNode);
                }

                if (AddClockwise(unitPos, unitSide, halfwidth, curAngle, out newPos, out newForward, out nextAngle))
                {
                    AIDataNode newNode = new AIDataNode();
                    newNode.m_MoveType = MoveType.RotateClockwise;
                    newNode.m_CostSoFar = (m_BaseRotate * Mathf.Rad2Deg) / 18.0f;
                    newNode.m_Parent = null;
                    newNode.m_Position = newPos;
                    newNode.m_Forward = newForward;
                    newNode.m_MoveAmount = m_BaseRotate * Mathf.Rad2Deg;
                    newNode.m_CurrentAngle = nextAngle;
                    newNode.m_DistanceToGoal = Vector3.Distance(m_Target.transform.position, newNode.m_Position);
                    m_DataNodes.AddFirst(newNode);
                }

                SortDataNodes();
                m_Run -= Time.deltaTime;
            }
        }
    }



    void SortDataNodes()
    {
        bool isSorted = false;
        LinkedList<AIDataNode> sorted = new LinkedList<AIDataNode>();
        do
        {
            LinkedListNode<AIDataNode> check = m_DataNodes.First;
            m_DataNodes.RemoveFirst();

            if(check == null)
            {
                isSorted = true;
                break;
            }
            if(m_ClosedList.Contains(check.Value))
            {
                continue;
            }

            LinkedListNode<AIDataNode> iter = sorted.First;
            bool added = false;
            while (!added && iter != null)
            {
                AIDataNode c = check.Value;
                AIDataNode i = iter.Value;
                if (c.m_Position == i.m_Position)
                    break;
                if(i.Compare(i,c) < 0)
                {
                    sorted.AddBefore(iter, c);
                    added = true;
                }
                else
                {
                    iter = iter.Next;
                    if(iter == null)
                    {
                        sorted.AddLast(c);
                        added = true;
                    }
                }
            }
            if(!added)
            {
                sorted.AddFirst(check.Value);
                added = true;
            }
        }
        while (!isSorted);

        m_DataNodes = sorted;
    }

    void OnDrawGizmos()
    {
        if (m_DataNodes.Count > 0)
        {
            foreach (AIDataNode node in m_DataNodes)
            {
                if (node.m_MoveType == MoveType.MoveForward)
                {
                    Vector3 heightMod = Vector3.up;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(node.m_Position + heightMod, 0.15f);

                    Gizmos.color = Color.green;
                    UnitMover unitMover = m_Unit.GetComponentInChildren<UnitMover>();
                    float ratio = 1 - (1 / (unitMover.parentUnit.Files - 1.5f));
                    float halfwidth = unitMover.parentUnit.Files * 0.5f * -ratio;
                    Vector3 unitRight = Vector3.Cross(node.m_Forward, Vector3.up).normalized;
                    Vector3 colliderOffset = new Vector3(0.0f, m_ColliderHeight) + node.m_Forward * -0.5f;
                    Vector3 tl = node.m_Position + (node.m_Forward * node.m_MoveAmount) - unitRight * halfwidth + colliderOffset;
                    Vector3 tr = node.m_Position + (node.m_Forward * node.m_MoveAmount) + unitRight * halfwidth + colliderOffset;
                    Gizmos.DrawWireSphere(tl, m_ColliderRadius);
                    Gizmos.DrawWireSphere(tr, m_ColliderRadius);
                    Gizmos.DrawLine(tl, tr);
                }
                else if (node.m_MoveType == MoveType.RotateClockwise || node.m_MoveType == MoveType.RotateCounterClockwise)
                {
                    Vector3 heightMod = Vector3.up;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(node.m_Position + heightMod, 0.15f);
                    Gizmos.DrawLine(node.m_Position + heightMod, node.m_Position + node.m_Forward);

                    Gizmos.color = Color.green;
                    UnitMover unitMover = m_Unit.GetComponentInChildren<UnitMover>();
                    float ratio = 1 - (1 / (unitMover.parentUnit.Files - 1.5f));
                    float halfwidth = unitMover.parentUnit.Files * 0.5f * -ratio;
                    Vector3 unitRight = Vector3.Cross(node.m_Forward, Vector3.up).normalized;
                    Vector3 colliderOffset = new Vector3(0.0f, m_ColliderHeight) + node.m_Forward * -0.5f;
                    Vector3 tl = node.m_Position - unitRight * halfwidth + colliderOffset;
                    Vector3 tr = node.m_Position + unitRight * halfwidth + colliderOffset;
                    Gizmos.DrawWireSphere(tl, m_ColliderRadius);
                    Gizmos.DrawWireSphere(tr, m_ColliderRadius);
                    Gizmos.DrawLine(tl, tr);
                }
            }
        }
    }
}
