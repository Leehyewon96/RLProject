using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{ 
    Coin,
    Gem,
    Bomb,
    Cannon,
    TreasureBox,
    MoneyTree,
   
    All,
    End,
}

public class ItemManager : MonoBehaviour
{
    private PhotonView photonView = null;
    List<Item> items = new List<Item>();

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void ShowItem(int num, Vector3 pos, ItemType itemType)
    {
        Debug.Log($"ShowItem");
        Vector3 randomPos = pos;
        randomPos.y += 1.2f;
        for (int i = 1; i <= num; ++i)
        {
            randomPos.x = Random.Range(pos.x - 1, pos.x + 1);
            randomPos.z = Random.Range(pos.z - 1, pos.z + 1);
            Item item = null;
            if (items.Count > 0)
            {
                item = items.Find(it => !it.gameObject.activeSelf && it.GetItemType() == itemType);
            }

            if (item == null)
            {
                string path = $"Prefabs/Item/{itemType.ToString()}";
                item = PhotonNetwork.Instantiate(path, randomPos, Quaternion.identity).GetComponent<Item>();
                items.Add(item);
            }

            item.transform.SetParent(transform);
            item.SetPosition(randomPos);
            item.SetActive(true);
        }
        //photonView.RPC("RPCShowItem", RpcTarget.AllBuffered, num, pos, itemType);
    }

    [PunRPC]
    public void RPCShowItem(int num, Vector3 pos, ItemType itemType)
    {
        if (photonView.IsMine && PhotonNetwork.IsMasterClient)
        {
            Vector3 randomPos = pos;
            randomPos.y += 1.2f;
            for (int i = 1; i <= num; ++i)
            {
                randomPos.x = Random.Range(pos.x - 1, pos.x + 1);
                randomPos.z = Random.Range(pos.z - 1, pos.z + 1);
                Item item = null;
                if (items.Count > 0)
                {
                    item = items.Find(it => !it.gameObject.activeSelf && it.GetItemType() == itemType);
                }

                if (item == null)
                {
                    string path = $"Prefabs/Item/{itemType.ToString()}";
                    item = PhotonNetwork.Instantiate(path, randomPos, Quaternion.identity).GetComponent<Item>();
                    items.Add(item);
                }

                item.transform.SetParent(transform);
                item.SetPosition(randomPos);
                item.SetActive(true);
            }
        }
    }
}
