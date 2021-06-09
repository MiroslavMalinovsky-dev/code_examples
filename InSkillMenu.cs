using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InSkillMenu : MonoBehaviour
{
    [Tooltip("Root GameObject of the menu used to toggle its activation")]
    public GameObject menuRoot;
    [Tooltip("Master volume when menu is open")]
    [Range(0.001f, 1f)]
    public float volumeWhenMenuOpen = 0.5f;
    [Tooltip("Text for image current balance of skill points")]
    public TMPro.TextMeshProUGUI skillPointsText;
    [Tooltip("Verification Window")]
    public GameObject verificationWindow;

    public GameObject[] treesOfSkill = new GameObject[3];

    [Header("UI for skill descriptions")]
    [Tooltip("Text for image current level name")]
    public TMPro.TextMeshProUGUI levelName;
    [Tooltip("Text for image current level value")]
    public TMPro.TextMeshProUGUI levelValue;
    [Tooltip("Text for image current effects description")]
    public TMPro.TextMeshProUGUI effectName1;
    [Tooltip("Text for image current effects description")]
    public TMPro.TextMeshProUGUI effectName2;
    [Tooltip("Text for image current effects description")]
    public TMPro.TextMeshProUGUI effectName3;

    public bool isVerification { get; set; }

    TMPro.TextMeshProUGUI[] effNamesArr;
    SkillLogicIndex[] m_SkillElements;
    SkillIndex[] m_UI_Indexes;
    float[] logIndexes;
    BalanceCounter m_balance;
    GameCursorManager m_gameCursor;
    SkillLinker skillToOpen;
    Button buttonOfSkill;
    AudioSource[] ui_Sounds;
    AudioSource ui_Sound1;
    AudioSource ui_Sound2;
    AudioSource ui_Sound3;
    SkillConnectorManager m_skillConnector;
    int ui_SkillIndex;

    void Start()
    {
        m_SkillElements = FindObjectsOfType<SkillLogicIndex>();
        m_UI_Indexes = FindObjectsOfType<SkillIndex>();

        logIndexes = new float[m_SkillElements.Length];
        for (int i = 0; i < m_SkillElements.Length; i++)
        {
            logIndexes[i] = m_SkillElements[i].logicIndex;
        }

        OffAlreadyOpenedSkillButton();

        treesOfSkill[0].SetActive(true);
        treesOfSkill[1].SetActive(false);
        treesOfSkill[2].SetActive(false);

        ui_Sounds = GetComponents<AudioSource>();
        ui_Sound1 = ui_Sounds[0];
        ui_Sound2 = ui_Sounds[1];
        ui_Sound3 = ui_Sounds[2];

        effNamesArr = new TMPro.TextMeshProUGUI[3];
        effNamesArr[0] = effectName1;
        effNamesArr[1] = effectName2;
        effNamesArr[2] = effectName3;

        m_balance = FindObjectOfType<BalanceCounter>();
        DebugUtility.HandleErrorIfNullFindObject<BalanceCounter, InSkillMenu>(m_balance, this);

        menuRoot.SetActive(false);
        m_gameCursor = FindObjectOfType<GameCursorManager>();
        DebugUtility.HandleErrorIfNullFindObject<GameCursorManager, InSkillMenu>(m_gameCursor, this);

        m_skillConnector = FindObjectOfType<SkillConnectorManager>();
    }

    void Update()
    {
        if (Input.GetButtonDown(GameConstants.k_ButtonSkillMenu)
            || (menuRoot.activeSelf && (Input.GetButtonDown(GameConstants.k_ButtonNameCancel)
                                     || Input.GetButtonDown(GameConstants.k_ButtonNamePauseMenu))))
        {
            SetPauseMenuActivation(!menuRoot.activeSelf);
        }

        skillPointsText.text = (m_balance.GetCurrentBalance()).ToString();
    }

    void OffAlreadyOpenedSkillButton()
    {
        if (m_UI_Indexes.Length != m_SkillElements.Length)
        {
            print("UI and Logic indexes are not equals");
            print(m_UI_Indexes.Length);
            print(m_SkillElements.Length);
            return;
        }

        for (int i = 0; i < m_SkillElements.Length; i++)
        {
            for (int k = 0; k < m_UI_Indexes.Length; k++)
            {
                if (m_UI_Indexes[k].index == m_SkillElements[i].logicIndex)
                {
                    SkillLinker m_SkillCurrentElement = m_SkillElements[i].GetComponent<SkillLinker>();
                    if (m_SkillCurrentElement)
                    {
                        if (m_SkillCurrentElement.isOpened)
                        {
                            Button m_ui_button = m_UI_Indexes[k].GetComponentInChildren<Button>();
                            m_ui_button.interactable = false;
                        }
                        m_SkillCurrentElement = null;
                    }
                }
            }   
        }
    }

    void SetPauseMenuActivation(bool active)
    {
        menuRoot.SetActive(active);

        if (menuRoot.activeSelf)
        {
            m_gameCursor.isMenuOpened = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            AudioUtility.SetMasterVolume(volumeWhenMenuOpen);

            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        { 
            if (!Input.GetButtonDown(GameConstants.k_ButtonNamePauseMenu))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
                m_gameCursor.isMenuOpened = false;
                AudioUtility.SetMasterVolume(1);
            }
        }

    }

    SkillLinker GetCurrentSkillElement(Button button)
    {
        ui_SkillIndex = button.GetComponentInParent<SkillIndex>().index;
        SkillLinker m_SkillCurrentElement = null;
        for (int i = 0; i < m_SkillElements.Length; i++)
        {
            if (ui_SkillIndex == m_SkillElements[i].logicIndex)
            {
                m_SkillCurrentElement = m_SkillElements[i].GetComponent<SkillLinker>();
                break;
            }
        }
        return m_SkillCurrentElement;
    }

    void ShowDescription(SkillLinker m_SkillCurrentElement)
    {
        levelName.text = m_SkillCurrentElement.GetCurrentLevelName();
        levelValue.text = "Level: " + m_SkillCurrentElement.GetCurrentLevelValue().ToString();
        string[] effectNames = m_SkillCurrentElement.GetCurrentEffectNames();
        for (int i = 0; i < effectNames.Length; i++)
        {
            effNamesArr[i].text = effectNames[i];
        }
    }

    void DeleteDescription()
    {
        levelName.text = string.Empty;
        levelValue.text = string.Empty;
        effectName1.text = string.Empty;
        effectName2.text = string.Empty;
        effectName3.text = string.Empty;
    }

    public void CloseSkillMenu()
    {
        SetPauseMenuActivation(false);
    }

    public void OnSkillButtonClick(Button button)
    {
        if (isVerification)
        {
            return;
        }

        SkillLinker m_SkillCurrentElement = GetCurrentSkillElement(button);

        if (m_SkillCurrentElement)
        {
            bool isCanOpen = m_SkillCurrentElement.CanOpenCurrentSkill();
            if (isCanOpen)
            {
                isVerification = true;
                verificationWindow.SetActive(true);
                ShowDescription(m_SkillCurrentElement);
                skillToOpen = m_SkillCurrentElement;
                buttonOfSkill = button;
            }
        }
    }

    public void OnOKButtonClick()
    {
        isVerification = false;
        skillToOpen.OpenCurrentSkill();
        skillToOpen = null;
        m_skillConnector.ResetSkills(ui_SkillIndex);
        ui_SkillIndex = 100;
        buttonOfSkill.interactable = false;
        buttonOfSkill = null;
        DeleteDescription();
    }

    public void OnCancelButtonClick()
    {
        isVerification = false;
        skillToOpen = null;
        buttonOfSkill = null;
        DeleteDescription();
    }

    public void OnSkillButtonSelect(Button button)
    {
        SkillLinker m_SkillCurrentElement = GetCurrentSkillElement(button);

        if (m_SkillCurrentElement && !isVerification)
        {
            ShowDescription(m_SkillCurrentElement);
        }
    }

    public void OnSkillButtonUnselect(Button button)
    {
        SkillLinker m_SkillCurrentElement = GetCurrentSkillElement(button);

        if (m_SkillCurrentElement && !isVerification)
        {
            DeleteDescription();
        }
    }

    public void OnClickAddPoints(int amount)
    {
        m_balance.AddPointsToBalance(amount);
    }

    public void PlaySFXOnOKClick()
    {
        ui_Sound1.Play();
    }

    public void PlaySFXOnButtonClick()
    {
        ui_Sound3.Play();
    }

    public void PlaySFXOnMoveCursor()
    {
        ui_Sound2.Play();
    }

}
