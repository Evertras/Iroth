using UnityEngine;
using System.Collections;

public enum MoveType
{
    RotateClockwise,
    RotateCounterClockwise,
    MoveForward,
}
public class AIDataNode : IComparer
{
    public MoveType m_MoveType;

    public float m_DistanceToGoal;
    public float m_CostSoFar;
    public float m_MoveAmount;

    public AIDataNode m_Parent = null;

    public Vector3 m_Position;
    public Vector3 m_Forward;
    public float m_CurrentAngle;

    public int Compare(object a, object b)
    {
        AIDataNode objA = (AIDataNode)a;
        AIDataNode objB = (AIDataNode)b;

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
