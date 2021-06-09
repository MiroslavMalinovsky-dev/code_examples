using UnityEngine;

public class DisplayMessage : MonoBehaviour
{
    [Tooltip("The text that will be displayed")]
    [TextArea]
    public string message;
    [Tooltip("Prefab for the message")]
    public GameObject messagePrefab;
    [Tooltip("Delay before displaying the message")]
    public float delayBeforeShowing;
    [Tooltip("Display message continuous")]
    public bool isContinuous;
    public bool m_WasDisplayed;
    public bool stopContinuous { get; set; }

    float m_InitTime = float.NegativeInfinity;
    
    DisplayMessageManager m_DisplayMessageManager;
    NotificationToast m_Notification;

    void Start()
    {
        m_InitTime = Time.time;
        m_DisplayMessageManager = FindObjectOfType<DisplayMessageManager>();
        DebugUtility.HandleErrorIfNullFindObject<DisplayMessageManager, DisplayMessage>(m_DisplayMessageManager, this);
    }

    void Update()
    {
        if (m_WasDisplayed)
        {
            if (isContinuous && stopContinuous)
            {
               m_Notification.StopContinuous();
               stopContinuous = false;
            }
            return;
        }

        if (Time.time - m_InitTime > delayBeforeShowing)
        {
            var messageInstance = Instantiate(messagePrefab, m_DisplayMessageManager.DisplayMessageRect);
            m_Notification = messageInstance.GetComponent<NotificationToast>();
            if (m_Notification)
            {
                if (isContinuous)
                {
                    m_Notification.InitializeContinuous(message);
                }
                else
                {
                    m_Notification.Initialize(message);
                }
            }
            
            m_WasDisplayed = true;
        }
    }
}
