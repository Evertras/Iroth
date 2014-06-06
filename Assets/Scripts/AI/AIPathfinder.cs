using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIPathfinder : MonoBehaviour
{
    float m_Run = 0.0f;

    LinkedList<AIDataNode> m_DataNodes;

    public GameObject m_Target = null;
    public GameObject m_Unit = null;

    public float m_BaseMove = 0.1f;
    public float m_BaseRotate = 18.0f * Mathf.Deg2Rad * 0.25f;
    public float m_ColliderHeight = 0.5f;
    public float m_ColliderRadius = 0.5f;

    // Use this for initialization
    void Start()
    {
        m_DataNodes = new LinkedList<AIDataNode>();
    }

    public void setRunFromMovement()
    {
        UnitMover unitMover = m_Unit.GetComponentInChildren<UnitMover>();
        m_Run = unitMover.parentUnit.maximumMovement;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Run > 0.0f && m_Target != null && m_Unit != null)
        {
            UnitMover unitMover = m_Unit.GetComponentInChildren<UnitMover>();
            Vector3 unitForward = m_Unit.transform.forward;
            Vector3 unitSide = m_Unit.transform.right;
            float ratio = 1 - (1 / (unitMover.parentUnit.Files - 1.5f));
            float halfwidth = unitMover.parentUnit.Files * 0.5f * -ratio;
            Vector3 unitPos = unitMover.parentUnit.transform.position;

            // no nodes to analyze
            if(m_DataNodes.Count == 0)
            {
                //attempt move forward
                Vector3 colliderOffset = new Vector3(0.0f, m_ColliderHeight) + unitForward * -0.5f;
                Vector3 tl = unitPos - unitSide * halfwidth + colliderOffset;
                Vector3 tr = unitPos + unitSide * halfwidth + colliderOffset;

                bool add = true;
                RaycastHit hitObj;
                if (Physics.CapsuleCast(tl, tr, m_ColliderRadius, unitForward, out hitObj, m_BaseMove))
                {
                    Debug.Log(hitObj.transform.tag + " hit at count of " + m_DataNodes.Count.ToString());
                    if (hitObj.transform.tag == "Obstacle")
                    {
                        add = false;
                    }
                }

                if(add)
                {
                    AIDataNode newNode = new AIDataNode();
                    newNode.m_MoveType = MoveType.MoveForward;
                    newNode.m_CostSoFar = m_BaseMove;
                    newNode.m_Parent = null;
                    newNode.m_Position = unitPos;
                    newNode.m_Forward = unitForward;
                    newNode.m_MoveAmount = m_BaseMove;
                    m_DataNodes.AddFirst(newNode);
                }

                //check if a turn CCW is possible
                float radius = unitMover.parentUnit.Files * 0.5f;
                float curAngle = unitMover.currentRotationAngle * Mathf.Deg2Rad;
                float nextAngle = curAngle + m_BaseRotate;

                Vector3 newRight = (new Vector3(Mathf.Cos(nextAngle), 0.0f, Mathf.Sin(nextAngle))).normalized;
                Vector3 newForward = Vector3.Cross(Vector3.up, newRight).normalized;

                Vector3 posChange = (unitPos - unitSide * halfwidth) - (unitPos - newRight * halfwidth);

                Vector3 newPos = unitPos + posChange;

                colliderOffset = new Vector3(0.0f, m_ColliderHeight) + newForward * -0.5f;
                tl = newPos - newRight * halfwidth + colliderOffset;
                tr = newPos + newRight * halfwidth + colliderOffset;

                add = true;
                if (Physics.CapsuleCast(tl, tr, m_ColliderRadius, newForward, out hitObj, m_BaseMove))
                {
                    Debug.Log(hitObj.transform.tag + " hit at count of " + m_DataNodes.Count.ToString());
                    if (hitObj.transform.tag == "Obstacle")
                    {
                        add = false;
                    }
                }

                if (add)
                {
                    AIDataNode newNode = new AIDataNode();
                    newNode.m_MoveType = MoveType.RotateCounterClockwise;
                    newNode.m_CostSoFar = m_BaseRotate;
                    newNode.m_Parent = null;
                    newNode.m_Position = newPos;
                    newNode.m_Forward = newForward;
                    newNode.m_MoveAmount = m_BaseRotate;
                    m_DataNodes.AddFirst(newNode);
                }

                //check if a turn CW is possible
                curAngle = unitMover.currentRotationAngle * Mathf.Deg2Rad;
                nextAngle = curAngle - m_BaseRotate;

                newRight = (new Vector3(Mathf.Cos(nextAngle), 0.0f, Mathf.Sin(nextAngle))).normalized;
                newForward = Vector3.Cross(Vector3.up, newRight).normalized;

                posChange = (unitPos + unitSide * halfwidth) - (unitPos + newRight * halfwidth);

                newPos = unitPos + posChange;

                colliderOffset = new Vector3(0.0f, m_ColliderHeight) + newForward * -0.5f;
                tl = newPos - newRight * halfwidth + colliderOffset;
                tr = newPos + newRight * halfwidth + colliderOffset;

                add = true;
                if (Physics.CapsuleCast(tl, tr, m_ColliderRadius, newForward, out hitObj, m_BaseMove))
                {
                    Debug.Log(hitObj.transform.tag + " hit at count of " + m_DataNodes.Count.ToString());
                    if (hitObj.transform.tag == "Obstacle")
                    {
                        add = false;
                    }
                }

                if (add)
                {
                    AIDataNode newNode = new AIDataNode();
                    newNode.m_MoveType = MoveType.RotateClockwise;
                    newNode.m_CostSoFar = m_BaseRotate;
                    newNode.m_Parent = null;
                    newNode.m_Position = newPos;
                    newNode.m_Forward = newForward;
                    newNode.m_MoveAmount = m_BaseRotate;
                    m_DataNodes.AddFirst(newNode);
                }
            }
        }
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
