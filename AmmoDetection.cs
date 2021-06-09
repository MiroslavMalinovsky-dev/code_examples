using UnityEngine;
using UnityEngine.UI;

public class AmmoDetection : MonoBehaviour
{
    [Tooltip("CanvasGroup to fade the ammo UI")]
    public CanvasGroup canvasGroup;
    [Tooltip("Text for image current ammo")]
    public TMPro.TextMeshProUGUI weaponAmmoText;
    [Tooltip("Text for image current magazines amount")]
    public TMPro.TextMeshProUGUI weaponMagazineText;
    [Tooltip("Sharpness for the fill ratio movements")]
    public float ammoFillMovementSharpness = 20f;

    WeaponController m_Weapon;

	void Start()
    {
        m_Weapon = GetComponentInParent<WeaponController>();
    }

    void Update()
    {
        weaponAmmoText.text = (m_Weapon.GetCurrentAmmo()).ToString();
        weaponMagazineText.text = (m_Weapon.GetCurrentMagazines()).ToString();   
    }
}
