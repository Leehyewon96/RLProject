using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HpBarManager : MonoBehaviour
{
    private List<HpBar> hpBars = new List<HpBar>();

    private void Awake()
    {
        hpBars = GetComponentsInChildren<HpBar>(true).ToList();
        Init();
    }

    public void Init()
    {
        foreach(HpBar hpbar in hpBars)
        {
            hpbar.SetActive(false);
            hpbar.UPdateIsUsed(false);
        }
    }

    public HpBar GetHpBar(HpBar.barType inType)
    {
        HpBar hpbar = hpBars.Find(bar => !bar.isUsed && bar.type == inType);
        if(hpbar == null)
        {
            switch(inType)
            {
                case HpBar.barType.Player:
                    hpbar = Instantiate(hpBars.FirstOrDefault(), transform);
                    break;
                case HpBar.barType.NPC:
                    hpbar = Instantiate(hpBars[1], transform);
                    break;
            }

            hpBars.Add(hpbar);
        }

        hpbar.SetActive(true);
        hpbar.UPdateIsUsed(true);
        return hpbar;
    }
}
