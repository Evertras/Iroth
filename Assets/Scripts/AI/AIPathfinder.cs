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

    public void HitTarget(AIDataNode endPoint)
    {
        m_Path = endPoint;
        m_PathList = GetPath();
    }

    List<AIDataNode> m_PathList;
    int m_CleanIndex = 0;
    AIDataNode pathShorterStart = null, pathShorterEnd = null;
    int optCorner = 0;
    public void CleanPath()
    {
        AIDataNode node = m_PathList[m_CleanIndex];

        UnitMover unitMover = m_Unit.GetComponentInChildren<UnitMover>();
        float halfwidth = unitMover.parentUnit.Files * 0.5f;
        Vector3 colliderOffset = new Vector3(0.0f, m_ColliderHeight) - node.m_Forward * 0.5f;
        Vector3 right = Vector3.Cross(Vector3.up, node.m_Forward).normalized;
        Vector3 tl = node.m_Position - right * halfwidth + colliderOffset;
        Vector3 tr = node.m_Position + right * halfwidth + colliderOffset;

        float maxValidDistance = 0.0f;
        AIDataNode validNode = null;
        int hitCorner = -1;
        Vector3 ctl = Vector3.zero, ctr = Vector3.zero;
        for(int i = m_CleanIndex + 1; i < m_PathList.Count; ++i)
        {
            AIDataNode checkNode = m_PathList[i];
            Vector3 cright = Vector3.Cross(Vector3.up, checkNode.m_Forward).normalized;
            ctl = checkNode.m_Position - right * halfwidth + colliderOffset;
            ctr = checkNode.m_Position + right * halfwidth + colliderOffset;

            float checkDistance = Vector3.Distance(node.m_Position, checkNode.m_Position);
            RaycastHit hitObj;
            if (Physics.SphereCast(node.m_Position, m_ColliderRadius, checkNode.m_Position - node.m_Position, out hitObj, checkDistance))
            {
                //Debug.Log(hitObj.collider.tag + " was hit pos when trying to optimize path " + i.ToString());
                hitCorner = 0;
                break;
            }
            else if (Physics.SphereCast(tr, m_ColliderRadius, ctr - tr, out hitObj, checkDistance))
            {
                //Debug.Log(hitObj.collider.tag + " was hit tr when trying to optimize path " + i.ToString());
                hitCorner = 1;
                break;
            }
            else if (Physics.SphereCast(tl, m_ColliderRadius, ctl - tl, out hitObj, checkDistance))
            {
                //Debug.Log(hitObj.collider.tag + " was hit tl when trying to optimize path " + i.ToString());
                hitCorner = 2;
                break;
            }
            else
            {
                validNode = checkNode;
                maxValidDistance = checkDistance;
            }
        }
        Debug.Log("Hit corner = " + hitCorner.ToString());
        pathShorterStart = node;
        pathShorterEnd = validNode;
        optCorner = hitCorner;

        if (optCorner == 1)
        {
            Vector3 angle = (ctl - tl).normalized;
            float angleRad = Mathf.Atan2(angle.z, angle.x) - Mathf.PI * 0.5f;
            // calc start turn node vals
            Vector3 newRight = (new Vector3(Mathf.Cos(angleRad), 0.0f, Mathf.Sin(angleRad))).normalized;
            Vector3 posChange = (node.m_Position + right * halfwidth) - (node.m_Position + newRight * halfwidth);
            // create start turn node
            AIDataNode startTurn = new AIDataNode();
            startTurn.m_CostSoFar = node.m_CostSoFar + Mathf.Abs(angleRad) * Mathf.Rad2Deg;
            startTurn.m_CurrentAngleRad = angleRad;
            startTurn.m_Position = node.m_Position + posChange;
            startTurn.m_DistanceToGoal = Vector3.Distance(startTurn.m_Position, m_Target.transform.position);
            startTurn.m_Forward = Vector3.Cross(newRight, Vector3.up).normalized;
            startTurn.m_MoveType = (angleRad < 0.0f? MoveType.RotateClockwise : MoveType.RotateCounterClockwise);
            startTurn.m_Parent = node;                            // link parent of start turn node to be node
            startTurn.CalcFCost();
            // create straight path node
            AIDataNode straight = new AIDataNode();
            straight.m_CostSoFar = startTurn.m_CostSoFar + Vector3.Distance(ctl, tl);
            straight.m_CurrentAngleRad = startTurn.m_CurrentAngleRad;
            straight.m_Position = startTurn.m_Position + (ctl - tl);
            straight.m_DistanceToGoal = Vector3.Distance(straight.m_Position, m_Target.transform.position);
            straight.m_MoveAmount = Vector3.Distance(ctl, tl);
            straight.m_Forward = startTurn.m_Forward;
            straight.m_MoveType = MoveType.MoveForward;
            straight.m_Parent = startTurn;                         // link parent of straight path node to be start turn node
            straight.CalcFCost();
            // calc end turn vals
            angleRad = validNode.m_CurrentAngleRad - straight.m_CurrentAngleRad;
            newRight = (new Vector3(Mathf.Cos(angleRad), 0.0f, Mathf.Sin(angleRad))).normalized;
            posChange = (straight.m_Position + right * halfwidth) - (straight.m_Position + newRight * halfwidth);
            // create end turn node
            AIDataNode endTurn = new AIDataNode();
            endTurn.m_CostSoFar = straight.m_CostSoFar + Mathf.Abs(angleRad) * Mathf.Rad2Deg;
            endTurn.m_CurrentAngleRad = angleRad;
            endTurn.m_Position = straight.m_Position + posChange;
            endTurn.m_DistanceToGoal = Vector3.Distance(endTurn.m_Position, m_Target.transform.position);
            endTurn.m_Forward = Vector3.Cross(newRight, Vector3.up).normalized;
            endTurn.m_MoveType = (angleRad < 0.0f ? MoveType.RotateClockwise : MoveType.RotateCounterClockwise);
            endTurn.m_Parent = straight;                                // link parent of end turn node to be straight path node
            endTurn.CalcFCost();
            // link parent of valid node to be end turn node
            validNode.m_Parent = endTurn;
        }
        else if (optCorner == 2)
        {
            Vector3 angle = (ctr - tr).normalized;
            float angleRad = Mathf.Atan2(angle.z, angle.x) - Mathf.PI * 0.5f;
            // calc start turn node vals
            Vector3 newRight = (new Vector3(Mathf.Cos(angleRad), 0.0f, Mathf.Sin(angleRad))).normalized;
            Vector3 posChange = (node.m_Position + right * halfwidth) - (node.m_Position + newRight * halfwidth);
            // create start turn node
            AIDataNode startTurn = new AIDataNode();
            startTurn.m_CostSoFar = node.m_CostSoFar + Mathf.Abs(angleRad) * Mathf.Rad2Deg;
            startTurn.m_CurrentAngleRad = angleRad;
            startTurn.m_Position = node.m_Position + posChange;
            startTurn.m_DistanceToGoal = Vector3.Distance(startTurn.m_Position, m_Target.transform.position);
            startTurn.m_Forward = Vector3.Cross(newRight, Vector3.up).normalized;
            startTurn.m_MoveType = (angleRad < 0.0f ? MoveType.RotateClockwise : MoveType.RotateCounterClockwise);
            startTurn.m_Parent = node;                            // link parent of start turn node to be node
            startTurn.CalcFCost();
            // create straight path node
            AIDataNode straight = new AIDataNode();
            straight.m_CostSoFar = startTurn.m_CostSoFar + Vector3.Distance(ctl, tl);
            straight.m_CurrentAngleRad = startTurn.m_CurrentAngleRad;
            straight.m_Position = startTurn.m_Position + (ctr - tr);
            straight.m_DistanceToGoal = Vector3.Distance(straight.m_Position, m_Target.transform.position);
            straight.m_MoveAmount = Vector3.Distance(ctl, tl);
            straight.m_Forward = startTurn.m_Forward;
            straight.m_MoveType = MoveType.MoveForward;
            straight.m_Parent = startTurn;                         // link parent of straight path node to be start turn node
            straight.CalcFCost();
            // calc end turn vals
            angleRad = validNode.m_CurrentAngleRad - straight.m_CurrentAngleRad;
            newRight = (new Vector3(Mathf.Cos(angleRad), 0.0f, Mathf.Sin(angleRad))).normalized;
            posChange = (straight.m_Position + right * halfwidth) - (straight.m_Position + newRight * halfwidth);
            // create end turn node
            AIDataNode endTurn = new AIDataNode();
            endTurn.m_CostSoFar = straight.m_CostSoFar + Mathf.Abs(angleRad) * Mathf.Rad2Deg;
            endTurn.m_CurrentAngleRad = angleRad;
            endTurn.m_Position = straight.m_Position + posChange;
            endTurn.m_DistanceToGoal = Vector3.Distance(endTurn.m_Position, m_Target.transform.position);
            endTurn.m_Forward = Vector3.Cross(newRight, Vector3.up).normalized;
            endTurn.m_MoveType = (angleRad < 0.0f ? MoveType.RotateClockwise : MoveType.RotateCounterClockwise);
            endTurn.m_Parent = straight;                                // link parent of end turn node to be straight path node
            endTurn.CalcFCost();
            // link parent of valid node to be end turn node
            validNode.m_Parent = endTurn;
        }
        m_PathList = GetPath();
    }

    public List<AIDataNode> GetPath()
    {
        List<AIDataNode> list = new List<AIDataNode>();
        list.Reverse();
        AIDataNode node = m_Path;
        while (node != null)
        {
            list.Add(node);
            node = node.m_Parent;
        }

        list.Reverse();
        return list;
    }

    public bool Step()
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
                AIDataNode newNode = new AIDataNode();

                //attempt move forward

                int check = AddForward(unitPos, unitForward, halfwidth);
                if (check >= 1)
                {
                    newNode = new AIDataNode();
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
                if(check == 2)
                {
                    HitTarget(newNode);
                    return true;
                }
                //Debug.Log("WTF " + check.ToString());

                //check if a turn CCW is possible
                float curAngle = unitMover.currentRotationAngle;

                Vector3 newPos, newForward;
                float nextAngle;
                check = AddCounterClockwise(unitPos, unitSide, halfwidth, curAngle, out newPos, out newForward, out nextAngle);
                if (check >= 1)
                {
                    newNode = new AIDataNode();
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
                if (check == 2)
                {
                    HitTarget(newNode);
                    return true;
                }

                //check if a turn CW is possible
                check = AddClockwise(unitPos, unitSide, halfwidth, curAngle, out newPos, out newForward, out nextAngle);
                if (check >= 1)
                {
                    newNode = new AIDataNode();
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
                if (check == 2)
                {
                    HitTarget(newNode);
                    return true;
                }
            }
            else
            {
                List<AIDataNode> newList = new List<AIDataNode>();
                int count = 0;
                AIDataNode newNode = new AIDataNode();
                while(m_DataNodes.Count > 0 && count < m_CountPerStep)
                {
                    count++;
                    AIDataNode node = m_DataNodes.First.Value;
                    m_DataNodes.RemoveFirst();
                    m_ClosedList.AddLast(node);

                    Vector3 unitForward = node.m_Forward;
                    Vector3 unitSide = Vector3.Cross(Vector3.up, unitForward).normalized;
                    Vector3 unitPos = node.m_Position;

                    int check = AddForward(unitPos, unitForward, halfwidth);
                    if (check >= 1)
                    {
                        newNode = new AIDataNode();
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
                    if (check == 2)
                    {
                        HitTarget(newNode);
                        return true;
                    }

                    float curAngle = node.m_CurrentAngleRad;

                    Vector3 newPos, newForward;
                    float nextAngle;
                    check = AddCounterClockwise(unitPos, unitSide, halfwidth, curAngle, out newPos, out newForward, out nextAngle);
                    if (check >= 1)
                    {
                        newNode = new AIDataNode();
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
                    if (check == 2)
                    {
                        HitTarget(newNode);
                        return true;
                    }

                    //check if a turn CW is possible
                    check = AddClockwise(unitPos, unitSide, halfwidth, curAngle, out newPos, out newForward, out nextAngle);
                    if (check >= 1)
                    {
                        newNode = new AIDataNode();
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
                    if (check == 2)
                    {
                        HitTarget(newNode);
                        return true;
                    }
                }
                foreach(AIDataNode node in newList)
                {
                    m_DataNodes.AddFirst(node);
                }
                SortDataNodes();
            }
        }
        return false;
    }

    int AddForward(Vector3 pos, Vector3 forward, float halfwidth)
    {
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 colliderOffset = new Vector3(0.0f, m_ColliderHeight) - forward * 0.5f;
        Vector3 tl = pos - right * halfwidth + colliderOffset;
        Vector3 tr = pos + right * halfwidth + colliderOffset;

        bool add = true;
        RaycastHit hitObj;
        int hitLayer = 1 << 9;
        if (Physics.CheckSphere(tl, m_ColliderRadius, ~hitLayer))
        {
            add = false;
        }
        if (Physics.CheckSphere(tr, m_ColliderRadius, ~hitLayer))
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
        if (Physics.CheckSphere(tl, m_ColliderRadius, 1 << 8) || Physics.CheckSphere(tr, m_ColliderRadius, 1 << 8) ||
            Physics.CheckSphere(pos, m_ColliderRadius, 1 << 8))
        {
            return 2;
        }

        return add ? 1 : 0;
    }

    int AddCounterClockwise(Vector3 pos, Vector3 right, float halfwidth, float curAngle, out Vector3 newPos, out Vector3 newForward, out float nextAngle)
    {
        float current = curAngle;
        nextAngle = current + m_BaseRotate;

        Vector3 newRight = (new Vector3(Mathf.Cos(nextAngle), 0.0f, Mathf.Sin(nextAngle))).normalized;
        newForward = Vector3.Cross(newRight, Vector3.up).normalized;

        Vector3 posChange = (pos - right * halfwidth) - (pos - newRight * halfwidth);

        newPos = pos + posChange;

        Vector3 colliderOffset = new Vector3(0.0f, m_ColliderHeight) - newForward * 0.5f;
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
        if (Physics.SphereCast(tr, m_ColliderRadius, tl - tr, out hitObj, halfwidth * 2.0f))
        {
            if (hitObj.transform.tag == "Obstacle")
            {
                add = false;
            }
        }
        if (Physics.CheckSphere(tl, m_ColliderRadius, 1 << 8) || Physics.CheckSphere(tr, m_ColliderRadius, 1 << 8) ||
            Physics.CheckSphere(pos, m_ColliderRadius, 1 << 8))
        {
            return 2;
        }

        return add ? 1 : 0;
    }
    int AddClockwise(Vector3 pos, Vector3 right, float halfwidth, float curAngle, out Vector3 newPos, out Vector3 newForward, out float nextAngle)
    {
        float current = curAngle;
        nextAngle = current - m_BaseRotate;

        Vector3 newRight = (new Vector3(Mathf.Cos(nextAngle), 0.0f, Mathf.Sin(nextAngle))).normalized;
        newForward = Vector3.Cross(newRight, Vector3.up).normalized;

        Vector3 posChange = (pos + right * halfwidth) - (pos + newRight * halfwidth);

        newPos = pos + posChange;

        Vector3 colliderOffset = new Vector3(0.0f, m_ColliderHeight) - newForward * 0.5f;
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
        if (Physics.SphereCast(tl, m_ColliderRadius, tr - tl, out hitObj, halfwidth * 2.0f))
        {
            if (hitObj.transform.tag == "Obstacle")
            {
                add = false;
            }
        }
        if (Physics.CheckSphere(tl, m_ColliderRadius, 1 << 8) || Physics.CheckSphere(tr, m_ColliderRadius, 1 << 8) ||
            Physics.CheckSphere(pos, m_ColliderRadius, 1 << 8))
        {
            return 2;
        }

        return add ? 1 : 0;
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
        if(pathShorterStart != null && pathShorterEnd != null)
        {
            UnitMover unitMover = m_Unit.GetComponentInChildren<UnitMover>();
            float halfwidth = unitMover.parentUnit.Files * 0.5f;
            Vector3 sunitRight = Vector3.Cross(pathShorterStart.m_Forward, Vector3.up).normalized;
            Vector3 scolliderOffset = new Vector3(0.0f, m_ColliderHeight) + pathShorterStart.m_Forward * -0.5f;
            Vector3 stl = pathShorterStart.m_Position + (pathShorterStart.m_Forward * m_BaseMove) - sunitRight * halfwidth + scolliderOffset;
            Vector3 str = pathShorterStart.m_Position + (pathShorterStart.m_Forward * m_BaseMove) + sunitRight * halfwidth + scolliderOffset;
            Vector3 eunitRight = Vector3.Cross(pathShorterEnd.m_Forward, Vector3.up).normalized;
            Vector3 ecolliderOffset = new Vector3(0.0f, m_ColliderHeight) + pathShorterEnd.m_Forward * -0.5f;
            Vector3 etl = pathShorterEnd.m_Position + (pathShorterEnd.m_Forward * m_BaseMove) - eunitRight * halfwidth + ecolliderOffset;
            Vector3 etr = pathShorterEnd.m_Position + (pathShorterEnd.m_Forward * m_BaseMove) + eunitRight * halfwidth + ecolliderOffset;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(stl, etl);
            Gizmos.DrawLine(str, etr);
            Gizmos.DrawLine(pathShorterStart.m_Position, pathShorterEnd.m_Position);

            Vector3 angle = Vector3.zero;
            if(optCorner == 2)
            {
                angle = (etr - str).normalized;
                float angleRad = Mathf.Atan2(angle.z, angle.x) - Mathf.PI * 0.5f;
                Gizmos.DrawLine(str, str + new Vector3(Mathf.Cos(angleRad), 0.0f, Mathf.Sin(angleRad)) * 10.0f);
            }
            else if(optCorner == 1)
            {
                angle = (etl - stl).normalized;
                float angleRad = Mathf.Atan2(angle.z, angle.x) - Mathf.PI * 0.5f;
                Gizmos.DrawLine(stl, stl + new Vector3(Mathf.Cos(angleRad), 0.0f, Mathf.Sin(angleRad)) * 10.0f);
            }

        }
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
                    float halfwidth = unitMover.parentUnit.Files * 0.5f;
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
                    float halfwidth = unitMover.parentUnit.Files * 0.5f;
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
