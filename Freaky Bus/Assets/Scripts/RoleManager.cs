using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles player entering the bus and role selection.
/// Roles are locked while both players are inside.
/// Attach to the Bus root GameObject.
/// </summary>
public class RoleManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private BusSeatManager busSeatManager;

    [Header("UI — Enter Button")]
    [SerializeField] private GameObject enterButtonPanel;
    [SerializeField] private Button enterButton;

    [Header("UI — Role Selection Panel")]
    [SerializeField] private GameObject rolePanel;
    [SerializeField] private Button driverButton;
    [SerializeField] private Button conductorButton;

    [Header("UI — Role Display")]
    [SerializeField] private TMP_Text roleText;

    private void Start()
    {
        enterButtonPanel.SetActive(false);
        rolePanel.SetActive(false);

        enterButton.onClick.AddListener(OnEnterClicked);
        driverButton.onClick.AddListener(OnDriverClicked);
        conductorButton.onClick.AddListener(OnConductorClicked);
    }

    private void OnDestroy()
    {
        enterButton.onClick.RemoveListener(OnEnterClicked);
        driverButton.onClick.RemoveListener(OnDriverClicked);
        conductorButton.onClick.RemoveListener(OnConductorClicked);
    }

    // ── Trigger Detection ──────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null || !pc.IsOwner) return;

        enterButtonPanel.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null || !pc.IsOwner) return;

        enterButtonPanel.SetActive(false);
        rolePanel.SetActive(false);
    }

    // ── Enter Button ───────────────────────────────────────────────

    private void OnEnterClicked()
    {
        enterButtonPanel.SetActive(false);
        rolePanel.SetActive(true);

        // Grey out taken roles — player can only pick available ones
        driverButton.interactable = !busSeatManager.IsDriverSeatTaken;
        conductorButton.interactable = !busSeatManager.IsConductorSeatTaken;
    }

    // ── Role Selection ─────────────────────────────────────────────

    private void OnDriverClicked()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        AssignRoleServerRpc(localId, true);
        rolePanel.SetActive(false);

        if (roleText != null)
            roleText.text = "Role: Driver";
    }

    private void OnConductorClicked()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        AssignRoleServerRpc(localId, false);
        rolePanel.SetActive(false);

        if (roleText != null)
            roleText.text = "Role: Conductor";
    }

    [ServerRpc(RequireOwnership = false)]
    private void AssignRoleServerRpc(ulong clientId, bool isDriver)
    {
        if (isDriver)
            busSeatManager.SitAsDriver(clientId);
        else
            busSeatManager.SitAsConductor(clientId);
    }

    // Called by BusSeatManager when a player exits to clear the role text
    public void ClearRoleText()
    {
        if (roleText != null)
            roleText.text = "";
    }
}