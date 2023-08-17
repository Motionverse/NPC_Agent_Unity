using MotionverseSDK;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ChatGPTClient : MonoBehaviour
{
    public string key = null;
    private List<string> m_Task = new List<string>();

    public async void GetDialogue(string str, NPC nPC)
    {
        if (m_Task.Count > 0)
            return;

        m_Task.Add(str);
        WebRequestDispatcher webRequestDispatcher = new();
        ChatGPTParams postData = new ChatGPTParams();
        postData.key = key;
        Messages messages = new Messages();
        messages.content = nPC.content;
        postData.messages.Add(messages);

        int startIndex = nPC.m_userMessage.Count - 3;
        if (startIndex < 0)
        {
            startIndex = 0;
        }

        for (int i = startIndex; i < nPC?.m_userMessage.Count; i++)
        {
            var msg = new Messages();
            msg.role = "user";
            msg.content = nPC?.m_userMessage[i];
            postData.messages.Add(msg);

            if (nPC?.m_assistantMessage.Count > i)
            {
                var assistantMessage = new Messages();
                assistantMessage.role = "assistant";
                assistantMessage.content = nPC?.m_assistantMessage[i];
                postData.messages.Add(assistantMessage);
            }
        }
        var newMessages = new Messages();
        newMessages.role = "user";
        newMessages.content = str;
        postData.messages.Add(newMessages);
       
        Debug.Log(JsonUtility.ToJson(postData));

        using var request = new UnityWebRequest("https://chatgpt.kazava.io/v1/chat/completions");
        request.timeout = 30000;
        byte[] jsonToSend = new UTF8Encoding().GetBytes(JsonUtility.ToJson(postData));
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);

        request.downloadHandler = new DownloadHandlerBuffer();
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.SetRequestHeader("Content-Type", "application/json");

        var asyncOperation = request.SendWebRequest();
        while (!asyncOperation.isDone)
        {
            await Task.Yield();
        }


        if (request.result == UnityWebRequest.Result.Success)
        {
            nPC?.m_userMessage.Add(str);
            HandleEnd(str, request.downloadHandler.text, nPC);
        }

        m_Task.Remove(str);


    }

    private static void HandleEnd(string str, string data, NPC nPC)
    {
        JObject obj = JObject.Parse(data);

        JArray choices = obj["choices"].ToObject<JArray>();
        if (choices.Count > 0)
        {
            string content = choices[0]["message"]["content"].ToString();
            nPC?.m_assistantMessage.Add(content);
            DriveTask driveTask = new DriveTask() { player = nPC.GetComponent<Player>(),text=content};
            TextDrive.GetDrive(driveTask);
        }

    }

}
[Serializable]
public class ChatGPTParams
{
    public string key = null;
    public List<Messages> messages = new List<Messages>();
}

[Serializable]
public class Messages
{
    public string role = "system";
    public string content = "";
}
