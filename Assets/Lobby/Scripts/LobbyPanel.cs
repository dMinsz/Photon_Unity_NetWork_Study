using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPanel : MonoBehaviour
{
    [SerializeField] RectTransform roomContent;
    [SerializeField] RoomEntry roomEntryPrefab;

    Dictionary<string, RoomInfo> roomDictionary;

    private void Awake()
    {
        roomDictionary = new Dictionary<string, RoomInfo>();
    }

    private void OnDisable()
    {
        roomDictionary.Clear();
    }

    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        // Clear room list
        for (int i = 0; i < roomContent.childCount; i++)
        {
            Destroy(roomContent.GetChild(i).gameObject);
        }

        // Update room info
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            { // 방이열려있지않으면 , 공개여부, 방이 사라질 예정
                if (roomDictionary.ContainsKey(info.Name))
                {
                    roomDictionary.Remove(info.Name);
                }

                continue;
            }

            // Update cached room info
            // 방이 변경됐을때 <-- 현재는 그냥 이름이있었던방이면
            if (roomDictionary.ContainsKey(info.Name))
            {
                roomDictionary[info.Name] = info;
            }
            // Add new room info to cache
            else
            {//새로운방
                roomDictionary.Add(info.Name, info);
            }
        }

        // Create room list // UI상 추가
        foreach (RoomInfo info in roomDictionary.Values)
        {
            RoomEntry entry = Instantiate(roomEntryPrefab, roomContent);
            entry.SetRoomInfo(info);
        }
    }

    public void LeaveLobby()
    {
        PhotonNetwork.LeaveLobby();
    }
}
