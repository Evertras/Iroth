using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AITester : MonoBehaviour
{
    public GameObject m_AIPather;
    GameObject m_Tester;
    AIPathfinder m_Pather;
    List<AIDataNode> m_Path = new List<AIDataNode>();
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            if (m_Tester == null)
            {
                m_Tester = (GameObject)GameObject.Instantiate((Object)m_AIPather);
                m_Tester.name = "_AITester";
                m_Pather = m_Tester.GetComponent<AIPathfinder>();

                m_Pather.m_Unit = GameObject.Find("TestUnit");
                m_Pather.m_Target = GameObject.Find("TestTarget");
            }
        }
        else if(Input.GetKey(KeyCode.B))
        {
            if(m_Pather.Step())
            {
                m_Path = m_Pather.GetPath();
            }
        }
    }

    void OnDrawGizmos()
    {
        if(m_Path.Count > 0)
        {
            Gizmos.color = Color.cyan;
            foreach(AIDataNode node in m_Path)
            {
                if(node.m_Parent != null)
                {
                    Gizmos.DrawLine(node.m_Position, node.m_Parent.m_Position);
                }
            }
        }
    }
}
