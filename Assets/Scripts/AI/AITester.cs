using UnityEngine;
using System.Collections;

public class AITester : MonoBehaviour
{
    public GameObject m_AIPather;
    GameObject m_Tester;
    AIPathfinder m_Pather;
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
        else if(Input.GetKeyUp(KeyCode.B))
        {
            m_Pather.setRunFromMovement();
        }
    }
}
