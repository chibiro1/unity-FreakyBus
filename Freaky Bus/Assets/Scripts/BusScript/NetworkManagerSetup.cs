using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class NetworkManagerSetup : MonoBehaviour
{
    public static NetworkManagerSetup Instance { get; private set; }

    public string SessionCode { get; private set; }
    public bool IsConnected { get; private set; }

    public string CurrentGameplaySceneName { get; private set; }

    private ISession currentSession;

    private void Awake()
    {
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        if (NetworkManager.Singleton != null)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
    else
    {
        Debug.LogError("NetworkManager.Singleton is missing! Is there a NetworkManager in your scene?");
    }

    await InitializeServices();
    }

    private void OnClientConnected(ulong clientId)
{
    if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
    {
        // Clients listen for the scene load to capture the name
        NetworkManager.Singleton.SceneManager.OnLoad += (id, sceneName, loadSceneMode, asyncOp) => {
            CurrentGameplaySceneName = sceneName;
            Debug.Log($"Client captured Scene Name: {sceneName}");
        };
    }
}

    private async Task InitializeServices()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log($"Signed in as: {AuthenticationService.Instance.PlayerId}");
    }

   public async Task<string> HostSession(int maxPlayers = 2, string sceneName = "Level1")
   {  
    try
    {
        // 1. Setup the session options
        var options = new SessionOptions
        {
            MaxPlayers = maxPlayers,
            IsLocked = false,
            IsPrivate = false,
        }.WithRelayNetwork();

        // 2. Actually create the session on the server
        currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        SessionCode = currentSession.Code;
        IsConnected = true;

        // 3. Store the name so PlayerController can find it later
        CurrentGameplaySceneName = sceneName; 

        // 4. Start the Host logic
        NetworkManager.Singleton.StartHost();

        // 5. NOW load the scene (NetworkSceneManager only works after StartHost)
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);

        Debug.Log($"Session created! Code: {SessionCode}");

        return SessionCode;
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Failed to host session: {e.Message}");
        return null;
    }
    }

    public async Task<bool> JoinSession(string code)
    {
        try
        {
            var options = new JoinSessionOptions();
            currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code, options);
            SessionCode = currentSession.Code;
            IsConnected = true;

            NetworkManager.Singleton.StartClient();

            Debug.Log($"Joined session: {SessionCode}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to join session: {e.Message}");
            return false;
        }
    }

    public async void LeaveSession()
    {
        if (currentSession != null)
        {
            await currentSession.LeaveAsync();
            currentSession = null;
        }

        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
            NetworkManager.Singleton.Shutdown();

        IsConnected = false;
        SessionCode = null;
    }
}