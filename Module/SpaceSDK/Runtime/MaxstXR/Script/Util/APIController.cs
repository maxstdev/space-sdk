using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using JsonFx.Json;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace maxstAR
{
    public class APIController : MonoBehaviour
    {
        static internal IEnumerator GET(string url, Dictionary<string, string> headers, int timeOut, 
            System.Action<string> completed, System.Action<string> fail)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            foreach (KeyValuePair<string, string> header in headers)
            {
                string headerKey = Escape(header.Key);
                string headerValue = Escape(header.Value);
                www.SetRequestHeader(headerKey, headerValue);
            }

            if (timeOut > 0)
            {
                www.timeout = timeOut;
            }

            yield return www.SendWebRequest();

            if (www.error != null)
            {
                fail(www.error);
            }
            else
            {
                completed(www.downloadHandler.text);
            }
            www.Dispose();
        }

        static internal IEnumerator GET(string url, Dictionary<string, string> headers, Dictionary<string, string> parameters, int timeOut, System.Action<string> completed, System.Action<string> fail)
        {
            if (parameters != null)
            {
                int current_length = 0;
                foreach (KeyValuePair<string, string> parameter in parameters)
                {
                    current_length++;
           
                    if(current_length == 1)
                    {
                        url = url + "?" + parameter.Key + "=" + parameter.Value;
                    }
                    else
                    {
                        url = url + "&" + parameter.Key + "=" + parameter.Value;
                    }
                }
            }

            UnityWebRequest www = UnityWebRequest.Get(url);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            foreach (KeyValuePair<string, string> header in headers)
            {
                string headerKey = Escape(header.Key);
                string headerValue = Escape(header.Value);
                www.SetRequestHeader(headerKey, headerValue);
            }

            if (timeOut > 0)
            {
                www.timeout = timeOut;
            }

            yield return www.SendWebRequest();

            if (www.error != null)
            {
                fail(www.error);
            }
            else
            {
                completed(www.downloadHandler.text);
            }
            www.Dispose();
        }

        static internal async Task GETAsync(string url, Dictionary<string, string> headers, Dictionary<string, string> parameters, int timeOut, 
            System.Action<string> completed, System.Action<string> fail)
        {
            if (parameters != null)
            {
                int current_length = 0;
                foreach (KeyValuePair<string, string> parameter in parameters)
                {
                    current_length++;

                    if (current_length == 1)
                    {
                        url = url + "?" + parameter.Key + "=" + parameter.Value;
                    }
                    else
                    {
                        url = url + "&" + parameter.Key + "=" + parameter.Value;
                    }
                }
            }

            UnityWebRequest www = UnityWebRequest.Get(url);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            foreach (KeyValuePair<string, string> header in headers)
            {
                string headerKey = Escape(header.Key);
                string headerValue = Escape(header.Value);
                www.SetRequestHeader(headerKey, headerValue);
            }

            if (timeOut > 0)
            {
                www.timeout = timeOut;
            }

            await www.SendWebRequest();

            if (www.error != null)
            {
                fail(www.error);
            }
            else
            {
                completed(www.downloadHandler.text);
            }
            www.Dispose();
        }

        static internal IEnumerator POST(string url, Dictionary<string, string> headers, Dictionary<string, string> parameters, int timeOut, System.Action<string> completed)
        {
            UploadHandler uploadHandler = null;
            WWWForm form = new WWWForm();

            if (headers.ContainsKey("Content-Type"))
            {
                if (headers["Content-Type"] == "application/json")
                {
                    if(parameters != null)
                    {
                        string jsonData = JsonWriter.Serialize(parameters);
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                        uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                    }
                }
                else
                {
                    form = new WWWForm();
                    if (parameters != null)
                    {
                        foreach (KeyValuePair<string, string> parameter in parameters)
                        {
                            form.AddField(parameter.Key, parameter.Value, System.Text.Encoding.UTF8);
                        }
                    }
                }
            }

            UnityWebRequest www = UnityWebRequest.Post(url, form);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            //SSO API 관??form?�서 uploadHandler null setting ??Bad Request ?�러 발생 ??
            if (uploadHandler != null) www.uploadHandler = uploadHandler;
            
            foreach (KeyValuePair<string, string> header in headers)
            {
                string headerKey = Escape(header.Key);
                string headerValue = Escape(header.Value);
                www.SetRequestHeader(headerKey, headerValue);
            }

#if UNITY_2017_3_OR_NEWER
            if (timeOut > 0)
            {
                www.timeout = timeOut;
            }

            yield return www.SendWebRequest();

            if (www.error != null && www.error != "")
            {
                completed(www.error);
            }
            else
            {
                completed(www.downloadHandler.text);
            }
#else
            yield return www.Send();

            if (www.isNetworkError)
            {
                completed(www.error);
            }
            else
            {
                completed(www.downloadHandler.text);
            }
#endif
            www.Dispose();
        }

        static internal async Task POSTAsync(string url, Dictionary<string, string> headers, Dictionary<string, string> parameters, int timeOut, System.Action<string> completed)
        {
            UploadHandler uploadHandler = null;
            WWWForm form = new WWWForm();

            if (headers.ContainsKey("Content-Type"))
            {
                if (headers["Content-Type"] == "application/json")
                {
                    if (parameters != null)
                    {
                        string jsonData = JsonWriter.Serialize(parameters);
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                        uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                    }
                }
                else
                {
                    form = new WWWForm();
                    if (parameters != null)
                    {
                        foreach (KeyValuePair<string, string> parameter in parameters)
                        {
                            form.AddField(parameter.Key, parameter.Value, System.Text.Encoding.UTF8);
                        }
                    }
                }
            }

            UnityWebRequest www = UnityWebRequest.Post(url, form);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            //SSO API 관??form?�서 uploadHandler null setting ??Bad Request ?�러 발생 ??
            if (uploadHandler != null) www.uploadHandler = uploadHandler;

            foreach (KeyValuePair<string, string> header in headers)
            {
                string headerKey = Escape(header.Key);
                string headerValue = Escape(header.Value);
                www.SetRequestHeader(headerKey, headerValue);
            }

#if UNITY_2017_3_OR_NEWER
            if (timeOut > 0)
            {
                www.timeout = timeOut;
            }

            await www.SendWebRequest();

            if (www.error != null && www.error != "")
            {
                completed(www.error);
            }
            else
            {
                completed(www.downloadHandler.text);
            }
#else
            yield return www.Send();

            if (www.isNetworkError)
            {
                completed(www.error);
            }
            else
            {
                completed(www.downloadHandler.text);
            }
#endif
            www.Dispose();
        }

        static string Escape(string text)
        {
            text = text.Replace("(", "&#40");
            text = text.Replace(")", "&#41");
            return text;
        }
    }
}
