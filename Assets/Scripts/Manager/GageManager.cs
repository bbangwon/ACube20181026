using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GageManager : Singleton<GageManager>{
    
    public Slider[] sliders = new Slider[4];
    public Image hpBar;
    
    void Awake()
    {
        for(int i=0; i<4; i++)
        {
            sliders[i] = transform.GetChild(i).GetComponent<Slider>();
        }
        hpBar = transform.GetChild(4).GetComponent<Image>();
    }
    
    public void SetHPBar(float hp)
    {
        hpBar.fillAmount = hp / 10;
    }
}
