using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RankUI : UIBase
{
    [SerializeField] private TextMeshProUGUI myRank = null;
    [SerializeField] protected GameObject rankElemParent = null;

    protected List<RankElem> rankElems = new List<RankElem>();

    protected virtual void Awake()
    {
        rankElems = GetComponentsInChildren<RankElem>(true).ToList();
        rankElems.ForEach(e => e.SetActive(false));
    }

    public void UpdateMyRank(string rank)
    {
        string postFix = "th";
        if (rank == "1")
        {
            postFix = "st";
        }
        else if (rank == "2")
        {
            postFix = "nd";
        }
        else if (rank == "3")
        {
            postFix = "rd";
        }
        myRank.SetText($"{rank}{postFix}");
    }

    public virtual void UpdateRank(string inName, string gemCnt, string rank)
    {
        var elem = rankElems.Find(e => e.GetName().Equals(inName));
        if (elem == null)
        {
            elem = rankElems.Find(e => !e.isAssigned);
            if(elem == null)
            {
                elem = Instantiate(rankElems.FirstOrDefault(), rankElemParent.transform);
                rankElems.Add(elem);
            }

            elem.SetIsAssigned(true);
        }

        elem.UpdateInfo(inName, gemCnt, rank);
        rankElems = rankElems.OrderBy(e => e.GetRank()).ToList();
        rankElems.ForEach((e) => e.SetActive(false));

        int lastIdx = rankElems.Count > 3 ? 3 : rankElems.Count;
        for (int i = 0; i < lastIdx; ++i)
        { 
            rankElems[i].SetActive(true);
            rankElems[i].gameObject.transform.SetSiblingIndex(i);
        }
    }
}
