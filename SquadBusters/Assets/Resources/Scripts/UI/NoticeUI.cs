using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum NoticeType
{
    MoneyTree,
    TreasureBox,
    Fusion,

    End,
}


public class NoticeUI : UIBase
{
    private List<NoticeElem> noticeElems = new List<NoticeElem>();

    private void Awake()
    {
        noticeElems = GetComponentsInChildren<NoticeElem>(true).ToList();
        noticeElems.ForEach(e => e.SetActive(false));
    }

    public NoticeElem ShowAcitveNotice(NoticeType type, bool isActive, GameObject target)
    {
        NoticeElem noticeElem = noticeElems.Find(e => !e.gameObject.activeSelf && e.GetNoticeType() == type);
        if(noticeElem == null)
        {
            NoticeElem origin = noticeElems.Find(e => e.GetNoticeType() == type);
            noticeElem = Instantiate(origin, transform);
            noticeElems.Add(noticeElem);
        }

        noticeElem.SetActive(isActive);
        noticeElem.SetTarget(target);
        return noticeElem;
    }
}
