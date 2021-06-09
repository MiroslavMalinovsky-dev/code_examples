using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;

public class SaveLoadGame : MonoBehaviour
{
	public Button loadButton;

	DisplayMessage m_message;
	
	void Start()
	{
		m_message = GetComponent<DisplayMessage>();
		m_message.m_WasDisplayed = true;

		if (!File.Exists(Application.persistentDataPath + "/SaveGameData.dat"))
	    {
	    	loadButton.interactable = false;
	    }
	    else
	    {
	    	loadButton.interactable = true;
	    }
	}

	public void SaveGame()
    {
    	BalanceCounter m_balance = FindObjectOfType<BalanceCounter>();
	    LocationsCounter m_locCounter = FindObjectOfType<LocationsCounter>();
	    FullSkiilTree m_skillTree = FindObjectOfType<FullSkiilTree>();

	    if (m_balance && m_locCounter && m_skillTree)
	    {
	    	BinaryFormatter bf = new BinaryFormatter();
    		FileStream file = File.Create(Application.persistentDataPath + "/SaveGameData.dat");
	    	SaveDataService data = new SaveDataService();

	    	SkillLinker[] m_skills = m_skillTree.GetComponentsInChildren<SkillLinker>();
	    	if (m_skills != null)
	    	{
	    		data.saveIsOpened = new int[m_skills.Length];
	    		for (int i = 0; i < m_skills.Length; i++)
	    		{
	    			data.saveIsOpened[i] = m_skills[i].isOpened ? 1 : 0;
	    		}
	    	}

	    	data.saveBalance = m_balance.balance;
	    	data.saveOpenedLocations = m_locCounter.openedLocationsCount;
	    	data.saveDiffLevel = m_locCounter.difficultLevel;

	    	bf.Serialize(file, data);
	    	file.Close();
	    	print("Game was saved!");
	    	m_message.message = "Game was saved!";
	    	m_message.m_WasDisplayed = false;
	    }
	    else
	    {
	    	print("There is no save game!");
	    	m_message.message = "There is no save game!";
	    	m_message.m_WasDisplayed = false;
	    }
    }

    public void LoadGame()
    {
    	BalanceCounter m_balance = FindObjectOfType<BalanceCounter>();
	    LocationsCounter m_locCounter = FindObjectOfType<LocationsCounter>();
	    FullSkiilTree m_skillTree = FindObjectOfType<FullSkiilTree>();

	    if (m_balance && m_locCounter && m_skillTree)
	    {
	    	if (File.Exists(Application.persistentDataPath + "/SaveGameData.dat"))
	    	{
	    		BinaryFormatter bf = new BinaryFormatter();
		    	FileStream file = File.Open(Application.persistentDataPath + "/SaveGameData.dat", FileMode.Open);
		    	SaveDataService data = (SaveDataService)bf.Deserialize(file);
		    	file.Close();

		    	SkillLinker[] m_skills = m_skillTree.GetComponentsInChildren<SkillLinker>();
		    	if (m_skills != null)
		    	{
		    		for (int i = 0; i < data.saveIsOpened.Length; i++)
		    		{
		    			m_skills[i].isOpened = (data.saveIsOpened[i] == 1) ? true : false;
		    		}
		    	}

		    	m_balance.balance = data.saveBalance;
		    	m_locCounter.openedLocationsCount = data.saveOpenedLocations;
		    	m_locCounter.difficultLevel = data.saveDiffLevel;

		    	print("Game was loaded!");
		    	m_message.message = "Game was loaded!";
	    		m_message.m_WasDisplayed = false;
	    	}
	    	else
	    	{
	    		print("There is no game for loading!");
	    	}
	    }
    }

    public void ResetGame()
    {
	    if (File.Exists(Application.persistentDataPath + "/SaveGameData.dat"))
	    {
	    	File.Delete(Application.persistentDataPath + "/SaveGameData.dat");
	    	print("Game was deleted!");
	    	m_message.message = "Old game was deleted!";
	    	m_message.m_WasDisplayed = false;
    	}
    }
    
}

[Serializable]
class SaveDataService
{
	public int saveBalance;
    public int saveOpenedLocations;
    public int saveDiffLevel;
    public int[] saveIsOpened;
}

