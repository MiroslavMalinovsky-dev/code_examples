using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillLinker : MonoBehaviour
{
    [Header("Common")]
    public int floor;
    public bool isOpened;

    [Header("I/O link connections")]
    public GameObject[] inputConnections;
    public GameObject[] outputConnections;

    [Header("Parameters of skill")]
    public GameObject skillTemplate;

    SkillTemplate skillParameters;
    BalanceCounter m_balance;
    SkillLogicIndex skillIndex;

    void Awake()
    {
        m_balance = FindObjectOfType<BalanceCounter>();
        DebugUtility.HandleErrorIfNullFindObject<BalanceCounter, SkillLinker>(m_balance, this);

        skillParameters = skillTemplate.GetComponent<SkillTemplate>();
        skillIndex = GetComponent<SkillLogicIndex>();
    }

    bool CheckPrevLinks()
    {
    	if (inputConnections == null)
        {
            return true;
        }

        foreach (GameObject link in inputConnections)
    	{
    		SkillLinker skill = link.GetComponent<SkillLinker>();
    		if (skill)
    		{
    			if (!skill.isOpened)
    			{
    				return false;
    			}
    		}
    		else
    		{
    			return false;
    		}
    	}
    	return true;
    }

    int GetCostOfSkill()
    {
        switch (skillParameters.levelValue)
        {
            case 1:
                return 50;

            case 2:
                return 100;

            case 3:
                return 200;

            default:
                return 0;
        }
    }

    void FindSkillReceiver()
    {
        SkillReceiver[] m_receiverArray = FindObjectsOfType<SkillReceiver>();
        if (m_receiverArray != null)
        {
            for (int i = 0; i < m_receiverArray.Length; i++)
            {
                if (m_receiverArray[i].endIndex == skillIndex.logicIndex)
                {
                    m_receiverArray[i].isOpened = isOpened;
                }
            }
        }
    }

    public bool CanOpenCurrentSkill()
    {
        int costSkill = GetCostOfSkill();
        print(CheckPrevLinks() && (m_balance.GetCurrentBalance() >= costSkill));
        return (CheckPrevLinks() && (m_balance.GetCurrentBalance() >= costSkill));
    }

    public void OpenCurrentSkill()
    {
        int costSkill = GetCostOfSkill();
        m_balance.SubPointsFromBalance(costSkill);
        isOpened = true;
        FindSkillReceiver();
    }

    public string GetCurrentLevelName()
    {
        return skillParameters.levelName;
    }

    public int GetCurrentLevelValue()
    {
        return skillParameters.levelValue;
    }

    public string[] GetCurrentEffectNames()
    {
        string[] effName = skillParameters.effectNames;
        return effName;
    }

    public float[] GetCurrentEffectValues()
    {
        float[] effValues = skillParameters.effectValues;
        return effValues;
    }
}
