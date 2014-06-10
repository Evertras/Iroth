using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MoveType
{
    RotateClockwise,
    RotateCounterClockwise,
    MoveForward,
}
public class AIDataNode
{
    public MoveType m_MoveType;

    public float m_DistanceToGoal;
    public float m_CostSoFar;
    public float m_MoveAmount;

    public AIDataNode m_Parent = null;

    public Vector3 m_Position;
    public Vector3 m_Forward;
    public float m_CurrentAngleRad;

    public float m_F;

    public void CalcFCost()
    {
        m_F = m_DistanceToGoal + m_CostSoFar;
    }
    private class sortAIDataHelper : IComparer<AIDataNode>
    {
        int IComparer<AIDataNode>.Compare(AIDataNode a, AIDataNode b)
        {
            AIDataNode objA = (AIDataNode)a;
            AIDataNode objB = (AIDataNode)b;

            int parentA = 0;
            AIDataNode temp = objA.m_Parent;
            while(temp != null)
            {
                parentA++;
                temp = temp.m_Parent;
            }
            int parentB = 0;
            temp = objB.m_Parent;
            while (temp != null)
            {
                parentB++;
                temp = temp.m_Parent;
            }
            if(parentA < parentB)
            {
                return -1;
            }
            else if (parentA > parentB)
            {
                return 1;
            }
            else
            {
                if (objA.m_F > objB.m_F)
                {
                    return -1;
                }
                else if (objA.m_F < objB.m_F)
                {
                    return 1;
                }
                else
                {
                    if (objA.m_DistanceToGoal < objB.m_DistanceToGoal)
                    {
                        return -1;
                    }
                    else if (objA.m_DistanceToGoal > objB.m_DistanceToGoal)
                    {
                        return 1;
                    }
                    else
                    {
                        if (objA.m_CostSoFar < objB.m_CostSoFar)
                        {
                            return -1;
                        }
                        else if (objA.m_CostSoFar > objB.m_CostSoFar)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
        }
    }

    public static IComparer<AIDataNode> GetComparer()
    {
        return (IComparer<AIDataNode>)new sortAIDataHelper();
    }
}
