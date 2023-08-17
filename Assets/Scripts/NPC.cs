
using MotionverseSDK;
using System.Collections.Generic;
using UnityEngine;


public class NPC : MonoBehaviour
{
    private delegate void TriggerEvent(Collider other);
    [SerializeField]
    private GameObject lookAt;
    [SerializeField]
    private GameObject questionMark;

    public string content = "";

    public Player player;

    [HideInInspector]
    public List<string> m_userMessage = new List<string>();
    [HideInInspector]
    public List<string> m_assistantMessage = new List<string>();


    #region 触发事件调用
    Vector3 pos;
    private void Start()
    {
        pos = new Vector3(lookAt.transform.position.x, lookAt.transform.position.y, lookAt.transform.position.z);
    }
    private void OnTriggerEnter(Collider other)
    {
        Player player = GetComponent<Player>();
        Audio2TextClient.instance.OnSwitchMic(true, player);
        Debug.Log("OnTriggerEnter");
    }

    private void OnTriggerStay(Collider other)
    {
        var playerCameraRoot = other.transform.Find("PlayerCameraRoot");
        if (playerCameraRoot)
        {
            lookAt.transform.position = Vector3.Lerp(lookAt.transform.position, new Vector3(playerCameraRoot.transform.position.x, lookAt.transform.position.y, playerCameraRoot.transform.position.z), 0.1f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        lookAt.transform.position = pos;
        Debug.Log("Exit");
        Audio2TextClient.instance.OnSwitchMic(false,null);
    }

    #endregion

  
}
