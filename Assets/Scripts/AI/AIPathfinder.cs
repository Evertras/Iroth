using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AIPathfinder : MonoBehaviour
{
    public float m_Run = 0.0f;

    LinkedList<AIDataNode> m_DataNodes;
    LinkedList<AIDataNode> m_ClosedList;
    AIDataNode m_Path;

    public GameObject m_Target = null;
    public GameObject m_Unit = null;

    public float m_BaseMove = 0.1f;
    public float m_BaseRotate = 18.0f * Mathf.Deg2Rad * 0.5f;
    public float m_ColliderHeight = 0.5f;
    public float m_ColliderRadius = 0.5f;

    public int m_CountPerStep = 20;

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

    public void Step()
    {
        if (m_Target != null && m_Unit != null)
        {
            UnitMover unitMover = m_Unit.GetComponentInChildren<UnitMover>();
            float halfwidth = unitMover.parentUnit.Files * 0.5f;
            if (m_DataNodes.Count == 0)
            {
                Vector3 unitForward = m_Unit.transform.forward;
                Vector3 unitSide = m_Unit.transform.right;
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
                    newNode.m_CurrentAngleRad = unitMover.currentRotationAngle;
                    newNode.m_DistanceToGoal = Vector3.Distance(m_Target.transform.position, newNode.m_Position);
                    newNode.CalcFCost();
                    bool shouldAdd = true;
                    foreach (AIDataNode close in m_ClosedList)
                    {
                        if (Vector3.Distance(newNode.m_Position, close.m_Position) < 0.00001f && newNode.m_CurrentAngleRad - close.m_CurrentAngleRad < 0.00001f)
                            shouldAdd = false;
                    }
                    if (shouldAdd)
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
                    newNode.m_CurrentAngleRad = nextAngle;
                    newNode.m_DistanceToGoal = Vector3.Distance(m_Target.transform.position, newNode.m_Position);
                    newNode.CalcFCost();
                    bool shouldAdd = true;
                    foreach(AIDataNode close in m_ClosedList)
                    {
                        if (Vector3.Distance(newNode.m_Position, close.m_Position) < 0.00001f && newNode.m_CurrentAngleRad - close.m_CurrentAngleRad < 0.00001f)
                            shouldAdd = false;
                    }
                    if(shouldAdd)
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
                    newNode.m_CurrentAngleRad = nextAngle;
                    newNode.m_DistanceToGoal = Vector3.Distance(m_Target.transform.position, newNode.m_Position);
                    newNode.CalcFCost();
                    bool shouldAdd = true;
                    foreach (AIDataNode close in m_ClosedList)
                    {
                        if (Vector3.Distance(newNode.m_Position, close.m_Position) < 0.00001f && newNode.m_CurrentAngleRad - close.m_CurrentAngleRad < 0.00001f)
                            shouldAdd = false;
                    }
                    if (shouldAdd)
                        m_DataNodes.AddFirst(newNode);
                }
            }
            else
            {
                List<AIDataNode> newList = new List<AIDataNode>();
                int count = 0;
                while(m_DataNodes.Count > 0 && count < m_CountPerStep)
                {
                    count++;
                    AIDataNode node = m_DataNodes.First.Value;
                    m_DataNodes.RemoveFirst();
                    m_ClosedList.AddLast(node);

                    Vector3 unitForward = node.m_Forward;
                    Vector3 unitSide = Vector3.Cross(Vector3.up, unitForward).normalized;
                    Vector3 unitPos = node.m_Position;

                    if (AddForward(unitPos, unitForward, halfwidth))
                    {
                        AIDataNode newNode = new AIDataNode();
                        newNode.m_MoveType = MoveType.MoveForward;
                        newNode.m_CostSoFar = node.m_CostSoFar + m_BaseMove;
                        newNode.m_Parent = node;
                        newNode.m_Position = unitPos + unitForward * m_BaseMove;
                        newNode.m_Forward = unitForward;
                        newNode.m_MoveAmount = m_BaseMove;
                        newNode.m_CurrentAngleRad = node.m_CurrentAngleRad;
                        newNode.m_DistanceToGoal = Vector3.Distance(m_Target.transform.position, newNode.m_Position);
                        newNode.CalcFCost();
                        bool shouldAdd = true;
                        foreach (AIDataNode close in m_ClosedList)
                        {
                            if (Vector3.Distance(newNode.m_Position, close.m_Position) < 0.00001f && newNode.m_CurrentAngleRad - close.m_CurrentAngleRad < 0.00001f)
                                shouldAdd = false;
                        }
                        if (shouldAdd)
                            newList.Add(newNode);
                    }

                    float curAngle = node.m_CurrentAngleRad;

                    Vector3 newPos, newForward;
                    float nextAngle;

                    if (AddCounterClockwise(unitPos, unitSide, halfwidth, curAngle, out newPos, out newForward, out nextAngle))
                    {
                        AIDataNode newNode = new AIDataNode();
                        newNode.m_MoveType = MoveType.RotateCounterClockwise;
                        newNode.m_CostSoFar = node.m_CostSoFar + (m_BaseRotate * Mathf.Rad2Deg) / 18.0f;
                        newNode.m_Parent = node;
                        newNode.m_Position = newPos;
                        newNode.m_Forward = newForward;
                        newNode.m_MoveAmount = m_BaseRotate * Mathf.Rad2Deg;
                        newNode.m_CurrentAngleRad = nextAngle;
                        newNode.m_DistanceToGoal = Vector3.Distance(m_Target.transform.position, newNode.m_Position);
                        newNode.CalcFCost();
                        bool shouldAdd = true;
                        foreach (AIDataNode close in m_ClosedList)
                        {
                            if (Vector3.Distance(newNode.m_Position, close.m_Position) < 0.00001f && newNode.m_CurrentAngleRad - close.m_CurrentAngleRad < 0.00001f)
                                shouldAdd = false;
                        }
                        if (shouldAdd)
                            newList.Add(newNode);
                    }

                    //check if a turn CW is possible
                    if (AddClockwise(unitPos, unitSide, halfwidth, curAngle, out newPos, out newForward, out nextAngle))
                    {
                        AIDataNode newNode = new AIDataNode();
                        newNode.m_MoveType = MoveType.RotateClockwise;
                        newNode.m_CostSoFar = node.m_CostSoFar + (m_BaseRotate * Mathf.Rad2Deg) / 18.0f;
                        newNode.m_Parent = node;
                        newNode.m_Position = newPos;
                        newNode.m_Forward = newForward;
                        newNode.m_MoveAmount = m_BaseRotate * Mathf.Rad2Deg;
                        newNode.m_CurrentAngleRad = nextAngle;
                        newNode.m_DistanceToGoal = Vector3.Distance(m_Target.transform.position, newNode.m_Position);
                        newNode.CalcFCost();
                        bool shouldAdd = true;
                        foreach (AIDataNode close in m_ClosedList)
                        {
                            if (Vector3.Distance(newNode.m_Position, close.m_Position) < 0.00001f && newNode.m_CurrentAngleRad - close.m_CurrentAngleRad < 0.00001f)
                                shouldAdd = false;
                        }
                        if (shouldAdd)
                            newList.Add(newNode);
                    }
                }
                foreach(AIDataNode node in newList)
                {
                    m_DataNodes.AddFirst(node);
                }
                SortDataNodes();
            }
        }
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
            if (hitObj.transform.tag == "Obstacle")
            {
                add = false;
            }
        }
        if (Physics.SphereCast(tl, m_ColliderRadius, tr - tl, out hitObj, halfwidth * 2.0f))
        {
            if (hitObj.transform.tag == "Obstacle")
            {
                add = false;
            }
        }

        return add;
    }

    bool AddCounterClockwise(Vector3 pos, Vector3 right, float halfwidth, float curAngle, out Vector3 newPos, out Vector3 newForward, out float nextAngle)
    {
        float current = curAngle;
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
            if (hitObj.transform.tag == "Obstacle")
            {
                add = false;
            }
        }

        return add;
    }
    bool AddClockwise(Vector3 pos, Vector3 right, float halfwidth, float curAngle, out Vector3 newPos, out Vector3 newForward, out float nextAngle)
    {
        float current = curAngle;
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
            if (hitObj.transform.tag == "Obstacle")
            {
                add = false;
            }
        }

        return add;
    }

    void SortDataNodes()
    {
        List<AIDataNode> list = new List<AIDataNode>();
        foreach(AIDataNode node in m_DataNodes)
        {
            list.Add(node);
        }
        IComparer<AIDataNode> comparer = AIDataNode.GetComparer();
        list.Sort(comparer);
        LinkedList<AIDataNode> sorted = new LinkedList<AIDataNode>();
        foreach(AIDataNode node in list)
        {
            sorted.AddFirst(node);
        }
        m_DataNodes = sorted;
    }

    void OnDrawGizmos()
    {
        int count = 0;
        if (m_DataNodes.Count > 0)
        {
            foreach (AIDataNode node in m_DataNodes)
            {
                if (node.m_MoveType == MoveType.MoveForward)
                {
                    Vector3 heightMod = Vector3.up;
                    if (count < m_CountPerStep)
                        Gizmos.color = Color.yellow;
                    else
                        Gizmos.color = Color.grey;
                    Gizmos.DrawWireSphere(node.m_Position + heightMod, 0.15f);

                    if (count < m_CountPerStep)
                        Gizmos.color = Color.green;
                    else
                        Gizmos.color = Color.clear;
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
                    if (count < m_CountPerStep)
                        Gizmos.color = Color.yellow;
                    else
                        Gizmos.color = Color.grey;
                    Gizmos.DrawWireSphere(node.m_Position + heightMod, 0.15f);
                    Gizmos.DrawLine(node.m_Position + heightMod, node.m_Position + node.m_Forward);

                    if (count < m_CountPerStep)
                        Gizmos.color = Color.green;
                    else
                        Gizmos.color = Color.clear;
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
                if(node.m_Parent != null)
                {
                    Gizmos.color = Color.blue;

                    Gizmos.DrawLine(node.m_Position, node.m_Parent.m_Position);
                }
                count++;
                if (count >= m_CountPerStep)
                    break;
            }
            //foreach (AIDataNode node in m_ClosedList)
            //{
            //    if (node.m_MoveType == MoveType.MoveForward)
            //    {
            //        Vector3 heightMod = Vector3.up;
            //        Gizmos.color = Color.cyan;
            //        Gizmos.DrawWireSphere(node.m_Position + heightMod, 0.15f);

            //        Gizmos.color = Color.blue;
            //        UnitMover unitMover = m_Unit.GetComponentInChildren<UnitMover>();
            //        float ratio = 1 - (1 / (unitMover.parentUnit.Files - 1.5f));
            //        float halfwidth = unitMover.parentUnit.Files * 0.5f * -ratio;
            //        Vector3 unitRight = Vector3.Cross(node.m_Forward, Vector3.up).normalized;
            //        Vector3 colliderOffset = new Vector3(0.0f, m_ColliderHeight) + node.m_Forward * -0.5f;
            //        Vector3 tl = node.m_Position + (node.m_Forward * node.m_MoveAmount) - unitRight * halfwidth + colliderOffset;
            //        Vector3 tr = node.m_Position + (node.m_Forward * node.m_MoveAmount) + unitRight * halfwidth + colliderOffset;
            //        Gizmos.DrawWireSphere(tl, m_ColliderRadius);
            //        Gizmos.DrawWireSphere(tr, m_ColliderRadius);
            //        Gizmos.DrawLine(tl, tr);
            //    }
            //    else if (node.m_MoveType == MoveType.RotateClockwise || node.m_MoveType == MoveType.RotateCounterClockwise)
            //    {
            //        Vector3 heightMod = Vector3.up;
            //        Gizmos.color = Color.cyan;
            //        Gizmos.DrawWireSphere(node.m_Position + heightMod, 0.15f);
            //        Gizmos.DrawLine(node.m_Position + heightMod, node.m_Position + node.m_Forward);

            //        Gizmos.color = Color.blue;
            //        UnitMover unitMover = m_Unit.GetComponentInChildren<UnitMover>();
            //        float ratio = 1 - (1 / (unitMover.parentUnit.Files - 1.5f));
            //        float halfwidth = unitMover.parentUnit.Files * 0.5f * -ratio;
            //        Vector3 unitRight = Vector3.Cross(node.m_Forward, Vector3.up).normalized;
            //        Vector3 colliderOffset = new Vector3(0.0f, m_ColliderHeight) + node.m_Forward * -0.5f;
            //        Vector3 tl = node.m_Position - unitRight * halfwidth + colliderOffset;
            //        Vector3 tr = node.m_Position + unitRight * halfwidth + colliderOffset;
            //        Gizmos.DrawWireSphere(tl, m_ColliderRadius);
            //        Gizmos.DrawWireSphere(tr, m_ColliderRadius);
            //        Gizmos.DrawLine(tl, tr);
            //    }
            //}
        }
    }
}
