using Photon.Pun;
using UnityEngine;

public class CharacterSpawner : Spawner
{
    [SerializeField] protected CharacterType characterType;

    protected override void Awake()
    {
        base.Awake();
        repeatInterval = 10f;
        if (characterType < CharacterType.Eggy)
        {
            SetPath($"Prefabs/Character/PlayerAttackCircle");
        }
        else
        {
            SetPath($"Prefabs/Character/NPCAttackCircle");
        }
    }

    protected override GameObject Spawn()
    {
        if (spawnObject == null || !spawnObject.activeSelf)
        {
            photonView.RPC("RPCEffect", RpcTarget.AllBuffered);

            GameObject obj = PhotonNetwork.Instantiate(path, transform.position, Quaternion.identity);

            if (obj.TryGetComponent<NPCAttackCircle>(out NPCAttackCircle attackCircle))
            {
                attackCircle.SpawnCharacter(transform.position, characterType);
            }
            return spawnObject = obj;
        }

        return null;
    }
}
