using UnityEngine;
using UnityEngine.Events;
using KtxUnity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.Networking;
using System.Threading;
using System;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class TextureManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField]
    private UnityEvent<float> OnProgressUpdated;

    [SerializeField]
    private UnityEvent OnProgressCompleted;

    [SerializeField]
    static public string TexturesDirectory;

    [SerializeField]
    private string TextureExtension = ".ktx2";

    [SerializeField]
    private string TextureJPGExtension = ".jpg";

    [SerializeField]
    private string TextureKTXExtension = ".ktx2";

    [SerializeField]
    private bool _loadOnTriggerEnter = true;

    [SerializeField]
    private bool _unloadOnTriggerExit = true;

    #endregion

    #region Private Fields

    private Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

    private HashSet<string> _loadingQueue = new HashSet<string>();

    private HashSet<string> _unloadingQueue = new HashSet<string>();

    private HashSet<string> _loadingInProgress = new HashSet<string>();

    private HashSet<string> _preventedToBeUnloaded = new HashSet<string>();

    private List<string> loadedTextures = new List<string>();

    private Dictionary<string, Texture2D> textures_8k = new Dictionary<string, Texture2D>();
    private List<string> loadedTextures_8k = new List<string>();

    private Dictionary<string, Texture2D> textures_4k = new Dictionary<string, Texture2D>();
    private List<string> loadedTextures_4k = new List<string>();

    private uint _numStartedLoading = 0u;

    private uint _numFinishedLoading = 0u;

    private bool isFirst = true;

    #endregion

    
    #region Private Properties
    private Dictionary<string, Texture2D> Textures
    {
        get
        {
            if (null == _textures)
            {
                _textures = new Dictionary<string, Texture2D>();
            }
            return _textures;
        }
    }

    private HashSet<string> LoadingQueue
    {
        get
        {
            if (null == _loadingQueue)
            {
                _loadingQueue = new HashSet<string>();
            }
            return _loadingQueue;
        }
    }

    private HashSet<string> UnloadingQueue
    {
        get
        {
            if (null == _unloadingQueue)
            {
                _unloadingQueue = new HashSet<string>();
            }
            return _unloadingQueue;
        }
    }

    private HashSet<string> LoadingInProgress
    {
        get
        {
            if (null == _loadingInProgress)
            {
                _loadingInProgress = new HashSet<string>();
            }
            return _loadingInProgress;
        }
    }

    private HashSet<string> PreventedToBeUnloaded
    {
        get
        {
            if (null == _preventedToBeUnloaded)
            {
                _preventedToBeUnloaded = new HashSet<string>();
            }
            return _preventedToBeUnloaded;
        }
    }

    public bool LoadOnTriggerEnter { get => _loadOnTriggerEnter; }

    public bool UnloadOnTriggerExit { get => _unloadOnTriggerExit; }

    private uint NumStartedLoading
    {
        get => _numStartedLoading;
        set
        {
            if (value == _numStartedLoading) { return; }
            _numStartedLoading = value;

            // notify progress updated
            OnProgressUpdated.Invoke(Progress);
        }
    }

    private uint NumFinishedLoading
    {
        get => _numFinishedLoading;
        set
        {
            if (value == _numFinishedLoading) { return; }
            _numFinishedLoading = value;

            // notify progress updated
            OnProgressUpdated.Invoke(Progress);

            if (Progress < 1.0f) { return; }

            // notify progress completed
            OnProgressCompleted.Invoke();

            _numStartedLoading = 0u;
            _numFinishedLoading = 0u;
        }
    }

    private float Progress
    {
        get
        {
            return NumStartedLoading > 0u ? NumFinishedLoading / (float)NumStartedLoading : 1.0f;
        }
    }

    private bool HasScheduledLoading => LoadingQueue.Count > 0;

    private bool HasScheduledUnloading => UnloadingQueue.Count > 0;

    private Dictionary<string, Texture2D[]> splitTextures = new Dictionary<string, Texture2D[]>();
    private Dictionary<string, Texture2D> splitBoundTextures = new Dictionary<string, Texture2D>();
    private Dictionary<string, int> readySplitBoundTextures = new Dictionary<string, int>();
    private string currentTexturePath = "";

    #endregion

    #region Unity Messages

    private void Start()
    {
        UnityThread.initUnityThread();
        GetComponent<Collider>().isTrigger = true; // it must be a trigger
        GetComponent<Rigidbody>().isKinematic = true; // it is not physical
    }

    async void Update()
    {
        if(TexturesDirectory != null)
        {
            // Perform loading only once at a time
            await ProcessScheduledLoading();
        }
    }

    void FixedUpdate()
    {
        // Perform unload only if there are no more scheduled loadings
        if (HasScheduledLoading) { return; }

        ProcessScheduledUnloading();
    }

    //void OnTriggerEnter(Collider other)
    //{
    //    if (!LoadOnTriggerEnter) { return; }

    //    var texturePath = GetTexturePath(other);
    //    if (IsNotLoaded(texturePath))
    //    {
    //        ScheduleLoading(texturePath);
    //    }
    //    else if (WillBeUnloaded(texturePath))
    //    {
    //        UnscheduleUnloading(texturePath);
    //    }
    //}

    //void OnTriggerExit(Collider other)
    //{
    //    if (!UnloadOnTriggerExit) { return; }

    //    var texturePath = GetTexturePath(other);
    //    if (IsLoaded(texturePath))
    //    {
    //        ScheduleUnloading(texturePath);
    //    }
    //    else if (WillBeLoaded(texturePath))
    //    {
    //        UnscheduleLoading(texturePath);
    //    }
    //}

    #endregion

    #region Public Methods

    public void ClearTexture()
    {

    }

    public void SetFirst(bool enable)
    {
        this.isFirst = enable;
    }

    public async Task<Texture2D> LoadTexture(string mapName, string povName, bool updateProgress = false)
    {
        var texturePath = "image2k" + "/" + povName;
        var texture = await LoadTextureByImage(texturePath, updateProgress);

        if (!Application.isEditor)
        {
            //Textures.Add(texturePath, texture);

            //if(loadedTextures.Contains(texturePath))
            //{
            //    loadedTextures.Remove(texturePath);
            //}
            //loadedTextures.Add(texturePath);
        }

        if (!Textures.ContainsKey(texturePath))
        {
            Textures.Add(texturePath, texture);
        }

        if (loadedTextures.Contains(texturePath))
        {
            loadedTextures.Remove(texturePath);
        }
        loadedTextures.Add(texturePath);


        if (loadedTextures.Count > 3 && !isFirst)
        {
            string textureString = loadedTextures[0];
            Texture2D removeTexture = Textures[textureString];
            Textures.Remove(textureString);
            Texture2D.DestroyImmediate(removeTexture);
            loadedTextures.Remove(textureString);
            System.GC.Collect();
        }

        return texture;
    }

    public IEnumerator LoadTextureCoroutine(string mapName, string povName, System.Action<Texture2D> complete, bool updateProgress = false)
    {
        var texturePath = "image2k" + "/" + povName;
        yield return StartCoroutine(LoadTextureByImageCoroutine(texturePath, complete: (returnTexture) => {

            complete(returnTexture);
        }));
    }

    public IEnumerator LoadTextureBackgroud(string mapName, string povName, string resolution, System.Action<Texture2D> complete)
    {
        var texturePath = Path.Combine(resolution, povName);
        yield return StartCoroutine(LoadTextureByImageCoroutine(texturePath, complete: (returnTexture) => {

            Dictionary<string, Texture2D> temp_textures = textures_8k;
            List<string> temp_loadedTextures = loadedTextures_8k;
            if (resolution == "image8k")
            {
                temp_textures = textures_8k;
                temp_loadedTextures = loadedTextures_8k;
            }
            else if (resolution == "image4k")
            {
                temp_textures = textures_4k;
                temp_loadedTextures = loadedTextures_4k;
            }

            if (!temp_textures.ContainsKey(texturePath))
            {
                temp_textures.Add(texturePath, returnTexture);
            }

            if (temp_loadedTextures.Contains(texturePath))
            {
                temp_loadedTextures.Remove(texturePath);
            }
            temp_loadedTextures.Add(texturePath);


            if (temp_loadedTextures.Count > 4)
            {
                string textureString = temp_loadedTextures[0];
                Texture2D removeTexture = temp_textures[textureString];
                temp_textures.Remove(textureString);
                Texture2D.DestroyImmediate(removeTexture);
                temp_loadedTextures.Remove(textureString);
                System.GC.Collect();
            }
            complete(returnTexture);
        }));

    }

    public IEnumerator LoadTextureBackgroud_array(string mapName, string povName, string resolution, System.Action<Texture2D[]> complete)
    {
        var texturePath = Path.Combine(resolution, povName);

        if (splitTextures.ContainsKey(texturePath))
        {
            Texture2D[] textures = splitTextures[texturePath];
            complete(textures);
        }
        else
        {
            yield return StartCoroutine(LoadTextureByImageCoroutine_array(texturePath, complete: (returnTexture) => {

                if (!splitTextures.ContainsKey(texturePath))
                {
                    splitTextures.Add(texturePath, returnTexture);
                }


                if (splitTextures.Count > 3)
                {
                    string removeKey = "";
                    foreach (string key in splitTextures.Keys)
                    {
                        if (key != currentTexturePath && key != texturePath)
                        {
                            removeKey = key;
                        }
                    }
                    Texture2D[] array_textures = splitTextures[removeKey];
                    foreach (Texture2D eachTexture in array_textures)
                    {
                        Texture2D.Destroy(eachTexture);
                    }
                    splitTextures.Remove(removeKey);
                }

                complete(returnTexture);
            }));
        }
    }

    public void ClearBoundTexture()
    {

        foreach (Texture2D eachTexture in splitBoundTextures.Values)
        {
            Texture2D removeTexture = eachTexture;
            Destroy(removeTexture);
            removeTexture = null;
        }


        splitBoundTextures.Clear();
        System.GC.Collect();
    }

    public IEnumerator LoadTextureBackgroud_bounds(string mapName, string povName, string resolution, int[] bounds, System.Action<int, Texture2D> process, System.Action complete)
    {
        var texturePath = resolution + "/" + povName;

        foreach (int bound in bounds)
        {
            string key = povName + "_" + bound;
            if (readySplitBoundTextures.ContainsKey(key) || splitBoundTextures.ContainsKey(key))
            {
                continue;
            }

            readySplitBoundTextures.Add(key, 1);
            if (!splitBoundTextures.ContainsKey(key))
            {
                yield return StartCoroutine(LoadTextureByImageCoroutine_bound(texturePath, bound, complete: (returnTexture) => {
                    readySplitBoundTextures.Remove(key);
                    if (!splitBoundTextures.ContainsKey(key))
                    {
                        splitBoundTextures.Add(key, returnTexture);

                        process(bound, returnTexture);
                    }
                }));
            }
        }

        complete();
    }

    public IEnumerator LoadTextureBackgroud_rotation_bounds(string mapName, string povName, string resolution, int[] bounds, System.Action<int, Texture2D> process, System.Action complete)
    {
        var texturePath = Path.Combine(mapName, resolution, povName);

        foreach (int bound in bounds)
        {
            string key = povName + "_" + bound;
            if (!splitBoundTextures.ContainsKey(key))
            {
                yield return StartCoroutine(LoadTextureByImageCoroutine_bound(texturePath, bound, complete: (returnTexture) => {

                    if (!splitBoundTextures.ContainsKey(key))
                    {
                        splitBoundTextures.Add(key, returnTexture);
                        process(bound, returnTexture);
                    }
                    else
                    {
                        Destroy(returnTexture);
                    }
                }));
            }
        }

        complete();
    }

    public IEnumerator LoadTextureBackgroud_array_ktx(string mapName, string povName, string resolution, System.Action<Texture2D[]> complete)
    {
        var texturePath = Path.Combine(resolution, povName);

        yield return new WaitForEndOfFrame();
        LoadTextureByImageCoroutine_array_ktx(texturePath, complete: (returnTexture) =>
        {
            complete(returnTexture);
        }).RunSynchronously();

    }

    public async Task UntilLoaded(string mapName, string povName)
    {
        var texturePath = Path.Combine(povName);
        await UntilLoaded(texturePath);
    }

    public async Task<Texture2D> GetLoadedTexture(string mapName, string povName)
    {
        var texturePath = "image2k"+"/"+ povName;
        await UntilLoaded(texturePath);
        return Textures[texturePath];
    }

    public void ScheduleIfNotLoaded(string mapName, string povName)
    {
        var texturePath = "image2k" + "/" + povName;
        if (IsNotLoaded(texturePath)) { ScheduleLoading(texturePath); }
    }

    public void PreventToBeUnloaded(string mapName, string povName)
    {
        var texturePath = Path.Combine(povName);
        PreventedToBeUnloaded.Add(texturePath);
    }

    public void AllowToBeUnloaded(string mapName, string povName)
    {
        var texturePath = Path.Combine(povName);
        PreventedToBeUnloaded.Remove(texturePath);
    }

    #endregion

    #region Private Methods

    private bool IsLoaded(string texturePath)
    {
        return Textures.ContainsKey(texturePath);
    }

    private bool IsNotLoaded(string texturePath)
    {
        return !IsLoaded(texturePath);
    }

    private async Task UntilLoaded(string texturePath)
    {
        while (IsNotLoaded(texturePath)) { await Task.Yield(); }
    }

    private string GetTexturePath(Collider other)
    {
        var povController = other.transform.parent;
        var povManager = povController.parent;
        var texturePath = Path.Combine(povManager.name, povController.name);
        return texturePath;
    }

    private string Dequeue(HashSet<string> q)
    {
        var v = q.First();
        q.Remove(v);
        return v;
    }

    private bool WillBeUnloaded(string texturePath)
    {
        return UnloadingQueue.Contains(texturePath);
    }

    private bool WillBeLoaded(string texturePath)
    {
        return LoadingQueue.Contains(texturePath);
    }

    private void ScheduleLoading(string texturePath)
    {
        if (LoadingQueue.Contains(texturePath)) { return; }

        if (LoadingInProgress.Contains(texturePath)) { return; }

        LoadingQueue.Add(texturePath);
    }

    private void UnscheduleLoading(string texturePath)
    {
        LoadingQueue.Remove(texturePath);
    }

    private void ScheduleUnloading(string texturePath)
    {
        UnloadingQueue.Add(texturePath);
    }

    private void UnscheduleUnloading(string texturePath)
    {
        UnloadingQueue.Remove(texturePath);
    }

    private IEnumerator LoadTextureByImageCoroutine(string texturePath, System.Action<Texture2D> complete)
    {
        texturePath = Path.Combine(TexturesDirectory, texturePath);
        texturePath = Path.ChangeExtension(texturePath, TextureJPGExtension);

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(texturePath);
        yield return www.SendWebRequest();
        Texture2D temp_texture = DownloadHandlerTexture.GetContent(www);

        complete(temp_texture);
        www.Dispose();
    }

    private IEnumerator LoadTextureByImageCoroutine_array(string texturePath, System.Action<Texture2D[]> complete)
    {
        texturePath = Path.Combine(TexturesDirectory, texturePath);

        List<Texture2D> textures = new List<Texture2D>();
        for (int i = 1; i < 9; i++)
        {
            string temp_texturePath = texturePath + "_" + i;
            temp_texturePath = Path.ChangeExtension(temp_texturePath, TextureJPGExtension);
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(temp_texturePath);
            SetHeaders(www);
            yield return www.SendWebRequest();
            Texture2D temp_texture = DownloadHandlerTexture.GetContent(www);
            temp_texture.wrapMode = TextureWrapMode.Clamp;
            textures.Add(temp_texture);
            www.Dispose();
        }

        complete(textures.ToArray());
    }

    private IEnumerator LoadTextureByImageCoroutine_bound(string texturePath, int number, System.Action<Texture2D> complete)
    {
        string imagePath = "";
        yield return StartCoroutine(maxstAR.XRAPI.Instance.GetVRImagePathCoroutine((result)=> {
            imagePath = result;
        }));

        
        string temp_texturePath = texturePath + "_" + number;
        temp_texturePath = Path.ChangeExtension(temp_texturePath, TextureJPGExtension);

        String combine_texturePath = imagePath.Replace("*", temp_texturePath);

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(combine_texturePath);
        SetHeaders(www);
        yield return www.SendWebRequest();

        Texture2D temp_texture = DownloadHandlerTexture.GetContent(www);
        temp_texture.wrapMode = TextureWrapMode.Clamp;
        complete(temp_texture);
        www.Dispose();
    }

    private async Task LoadTextureByImageCoroutine_array_ktx(string texturePath, System.Action<Texture2D[]> complete)
    {
        texturePath = Path.Combine(TexturesDirectory, texturePath);

        List<Texture2D> textures = new List<Texture2D>();
        for (int i = 1; i < 9; i++)
        {
            string temp_texturePath = texturePath + "_" + i;
            temp_texturePath = Path.ChangeExtension(temp_texturePath, TextureKTXExtension);
          
            var textureResult = await new KtxTexture().LoadFromUrl(temp_texturePath, true);
            textures.Add(textureResult.texture);
        }

        complete(textures.ToArray());
    }

    private async Task<Texture2D> LoadTextureByImage(string texturePath, bool updateProgress = true)
    {
        string ImagePath = await maxstAR.XRAPI.Instance.GetVRImagePathAsync();

        String combine_texturePath = ImagePath.Replace("*", texturePath + ".jpg");

        if (updateProgress) { NumStartedLoading += 1u; }

        List<Texture2D> textures = new List<Texture2D>();
        for (int i = 0; i < 1; i++)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(combine_texturePath);
            SetHeaders(www);
            await www.SendWebRequest();

            Texture2D temp_texture = DownloadHandlerTexture.GetContent(www);
            textures.Add(temp_texture);
            www.Dispose();
        }

        Texture2D texture = textures[0];

        if (updateProgress) { NumFinishedLoading += 1u; }


        return texture;
    }

    private async Task<Texture2D> LoadTexture(string texturePath, bool updateProgress = true)
    {
        texturePath = Path.Combine(TexturesDirectory, texturePath);
        texturePath = Path.ChangeExtension(texturePath, TextureExtension);
        if (updateProgress) { NumStartedLoading += 1u; }

        var textureResult = await new KtxTexture().LoadFromUrl(texturePath, true);

        if (updateProgress) { NumFinishedLoading += 1u; }
        return textureResult.texture;
    }

    private async Task ProcessScheduledLoading()
    {
        var texturePath = string.Empty;
        var isLoaded = true;

        while (LoadingQueue.Count > 0 && isLoaded)
        {
            texturePath = Dequeue(LoadingQueue);
            isLoaded = IsLoaded(texturePath);
        }

        if (isLoaded) { return; }

        if (!LoadingInProgress.Contains(texturePath))
        {
            LoadingInProgress.Add(texturePath);

            //Debug.Log(texturePath);

            var texture = await LoadTextureByImage(texturePath);

            if (!Textures.ContainsKey(texturePath))
            {
                Textures.Add(texturePath, texture);
            }

            if (loadedTextures.Contains(texturePath))
            {
                loadedTextures.Remove(texturePath);
            }
            loadedTextures.Add(texturePath);


            if (loadedTextures.Count > 4 && !isFirst)
            {
                string textureString = loadedTextures[0];
                Texture2D removeTexture = Textures[textureString];
                Textures.Remove(textureString);

                if(Application.isEditor)
                {
                    DestroyImmediate(removeTexture);
                }
                else
                {
                    Destroy(removeTexture);
                }
                
                loadedTextures.Remove(textureString);
                System.GC.Collect();
            }

            LoadingInProgress.Remove(texturePath);
        }
    }

    private void ProcessScheduledUnloading()
    {
        var texturePath = string.Empty;
        var isNotLoaded = true;

        var postponed = new HashSet<string>();
        while (HasScheduledUnloading && isNotLoaded)
        {
            texturePath = Dequeue(UnloadingQueue);
            if (PreventedToBeUnloaded.Contains(texturePath))
            {
                postponed.Add(texturePath);
            }
            isNotLoaded = IsNotLoaded(texturePath);
        }
        UnloadingQueue.UnionWith(postponed);

        if (isNotLoaded || postponed.Contains(texturePath)) { return; }

        // unload the texture
        Destroy(_textures[texturePath]);
        Textures.Remove(texturePath);
    }

    public void SetHeaders(UnityWebRequest www)
    {
        if(maxstAR.XRAPI.Instance.GetAccessToken() == "")
        {
            maxstAR.XRAPI.Instance.MakeAccessToken();
        }
        foreach (var header in maxstAR.XRAPI.Instance.GetHeaders())
        {
            www.SetRequestHeader(header.Key, header.Value);
        }
    }

    #endregion
}
