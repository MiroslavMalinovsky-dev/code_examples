using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceCounter : MonoBehaviour
{
	public int balance;
    public float increaseOnPer = 0f;

    SkillReceiver[] m_skillRecArr;

    void Start()
    {
        m_skillRecArr = GetComponents<SkillReceiver>();
    }

    public void OnSelectThisSkill(int ui_Index)
    {
        if (m_skillRecArr != null)
        {
            if (m_skillRecArr[0].isOpened &&
                (m_skillRecArr[0].endIndex == ui_Index || ui_Index == 0))
            {
                increaseOnPer = m_skillRecArr[0].receivedValues[0];
            }

            if (m_skillRecArr[1].isOpened &&
                (m_skillRecArr[1].endIndex == ui_Index || ui_Index == 0))
            {
                increaseOnPer = m_skillRecArr[1].receivedValues[0];
            }
        }
    }

    public void AddPointsToBalance(int amount)
    {
    	balance = balance + (int)Mathf.Ceil(amount * (1f + increaseOnPer));
    }

    public void SubPointsFromBalance(int skillCost)
    {
    	if (balance - skillCost < 0f)
    	{
    		print("Negative Balance!!!");
            return;
    	}
    	balance -= skillCost;	
    }

    public void ResetBalance()
    {
        balance = 0;
    }

    public int GetCurrentBalance()
    {
    	return balance;
    }
}
