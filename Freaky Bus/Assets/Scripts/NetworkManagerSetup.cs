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
        await InitializeServices();
    }

    private async Task InitializeServices()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log($"Signed in as: {AuthenticationService.Instance.PlayerId}");
    }

    public async Task<string> HostSession(int maxPlayers = 2, string sceneName = "Gameplay")
    {
        try
        {
            var options = new SessionOptions
            {
                MaxPlayers = maxPlayers,
                IsLocked = false,
                IsPrivate = false,
            }.WithRelayNetwork();

            currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            SessionCode = currentSession.Code;
            IsConnected = true;

            NetworkManager.Singleton.StartHost();
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