using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace MotionverseSDK
{
    public class AudioUrlDrive
    {
        private const string TAG = nameof(AudioUrlDrive);
        public static Action<Drive> CallBack;
        private static Dictionary<string, Drive> m_cacheDrive = new Dictionary<string, Drive>();//缓存

        private static List<string> m_waitTask = new List<string>();//等待列表
        private static List<string> m_curTask = new List<string>();//当前正在加载列表
    

        public static void GetDrive(string text)
        {
            CastTask(text);
        }
        private static async void CastTask(string text, string requestId = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                if (m_waitTask.Count == 0)
                {
                    return;//没有等待下载的任务
                }
                text = m_waitTask[0];
                m_waitTask.RemoveAt(0);
            }
            if (m_curTask.Count > 0)
            {
                m_waitTask.Add(text);
            }
            else
            {
                m_curTask.Add(text);
                WebRequestDispatcher webRequestDispatcher = new WebRequestDispatcher();
                AudioUrlMotionParams postData = new AudioUrlMotionParams()
                {
                    audio_url = text,
                };
                //postData.face_config.face_type = Player.instance.faceType;
                //postData.body_config.body_motion = Player.instance.bodyMotion;
                //postData.body_config.body_compress = true;
                //postData.body_config.body_head_x_rot = Player.instance.bodyHeadXRot;

                Debug.Log(JsonUtility.ToJson(postData));
                WebResponse response = await webRequestDispatcher.Post(Config.AudioMotionUrl, JsonUtility.ToJson(postData), new CancellationTokenSource().Token);

                if (response.code == 200)
                {
                    Debug.Log(response.data);
                    JObject obj = JObject.Parse(response.data);
                    if (int.Parse(obj["code"].ToString()) == 0)
                    {
                        string audioUrl = text;
                        string faceUrl = obj["data"]["allfaceData"]["oss_url"].ToString();
                        string motionUrl = obj["data"]["motionFusionedData"]["oss_url"].ToString();
                      
                        if (!m_cacheDrive.TryGetValue(audioUrl,out Drive drive)){
                            m_cacheDrive.Add(audioUrl, new Drive());
                        }
                        else
                        {

                            CallBack?.Invoke(drive);

                            CastTask(null);
                        }
                        AudioDownLoad(audioUrl);
                        FaceDownLoad(faceUrl, audioUrl);
                        MotionDownLoad(motionUrl, audioUrl);
                        m_curTask.Remove(text);

                        //if (bool.Parse(obj["data"]["is_request"].ToString()))
                        //{
                        //    //Debug.Log("继续当前任务");
                        //    CastTask(text, obj["data"]["request_id"].ToString());
                        //}
                        //else
                        //{
                        //    //Debug.Log("当前任务结束");
                        //    CastTask(null);
                        //}

                        CastTask(null);
                    }
                }
                else
                {
                    Debug.Log(response.msg);
                }
            }

        }

        private static async void AudioDownLoad(string audioUrl)
        {
            AudioDownloader audioDownloader = new AudioDownloader();
            CancellationToken token = new CancellationToken();
            AudioType audioType = AudioType.WAV;
            if (audioUrl.Contains("mp3") || audioUrl.Contains("MP3"))
            {
                audioType = AudioType.MPEG;
            }
            AudioClip clip = await audioDownloader.Execute(audioUrl, audioType, token);
            if (m_cacheDrive.TryGetValue(audioUrl, out Drive drive))
            {
                drive.clip= clip;
                drive.step++;
                //SDKLogger.Log(TAG, "音频下载成功");

                if(drive.step == 3)
                {
                    CallBack?.Invoke(drive);
                }
            }

           

        }

        private static async void FaceDownLoad(string faceUrl,string key)
        {
            JsonDownloader jsonDownloader = new JsonDownloader();
            CancellationToken token = new CancellationToken();
            string data = await jsonDownloader.Execute(faceUrl, token);
            if (m_cacheDrive.TryGetValue(key, out Drive drive))
            {
                drive.bsData= data;
                drive.step++;
                //SDKLogger.Log(TAG, "动作数据下载成功");
                if (drive.step == 3)
                {
                    CallBack?.Invoke(drive);
                }
            }
        }

        private static async void MotionDownLoad(string motionUrl, string key)
        {
            JsonDownloader jsonDownloader = new JsonDownloader();
            CancellationToken token = new CancellationToken();
            byte[] data = await jsonDownloader.ExecuteBuffer(motionUrl, token);
            if (m_cacheDrive.TryGetValue(key, out Drive drive))
            {
                drive.motionByte = data;
                drive.step++;
                //SDKLogger.Log(TAG, "动作数据下载成功");
                if(drive.step == 3)
                {
                    CallBack?.Invoke(drive);
                }
            }
        }





    }
}
