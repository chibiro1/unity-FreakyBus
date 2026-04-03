using TMPro;
using Unity.Netcode;
using UnityEngine;

public class SessionCodeDisplay : NetworkBehaviour
{
    [SerializeField] private TMP_Text codeText;


    public override void OnNetworkSpawn()
{
    if (codeText == null) return;
    codeText.text = $"Code: {NetworkManagerSetup.Instance.SessionCode}";
}

    // public override void OnNetworkSpawn()
    // {
    //     if (codeText == null) return;

    //     if (!IsHost)
    //     {
    //         codeText.gameObject.SetActive(false);
    //         return;
    //     }

    //     codeText.text = $"Code: {NetworkManagerSetup.Instance.SessionCode}";
    // }
}