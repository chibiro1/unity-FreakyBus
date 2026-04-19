using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class SanityManager : NetworkBehaviour
{
    [Header("Settings")]
    public float maxSanity = 100f;
    public float drainRate = 5f;
    public float drainDelay = 1.5f;
    
    [Header("Feedback")]
    public float flashSpeed = 4f;
    public Sprite[] sanityFrames; 

    private float currentSanity;
    private float lastSanity; 
    private float currentExposureTimer = 0f;

    private PlayerUIReferences ui;
    private HashSet<PassengerAI> nearbyAnomalies = new HashSet<PassengerAI>();

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { enabled = false; return; }
        currentSanity = maxSanity;
        lastSanity = maxSanity;
        Invoke(nameof(LinkUI), 0.5f);
    }

    private void LinkUI()
    {
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) ui = pc.LocalUI;
        if (ui != null && ui.kickOutButton != null)
            ui.kickOutButton.onClick.AddListener(RequestKickOut);
    }

    private void Update()
    {
        if (ui == null) return;

        // 1. Identify anomalies currently on the bus
        int busAnomalies = 0;
        foreach (var p in nearbyAnomalies)
        {
            if (p != null && (p.IsBoarding || p.IsSeated)) busAnomalies++;
        }

        // 2. Logic: Only drain and show UI if anomalies are on the bus
        if (busAnomalies > 0)
        {
            currentExposureTimer += Time.deltaTime;
            if (currentExposureTimer >= drainDelay)
            {
                currentSanity -= drainRate * busAnomalies * Time.deltaTime;
                UpdateUI(true); // Update and Show
            }
        }
        else
        {
            currentExposureTimer = 0f;
            UpdateUI(false); // Hide bars if no anomalies on bus
        }

        HandleDamageFeedback(busAnomalies);
        UpdateKickButton(busAnomalies > 0);
    }

    private void UpdateUI(bool isVisible)
    {
        if (ui == null || ui.sanityBarImages.Length < 2) return;

        // UI 0 = Conductor/Walking | UI 1 = Driver
        bool isDriving = ui.busDriverPanel.activeInHierarchy;

        // Visibility Toggling
        ui.sanityBarImages[0].gameObject.SetActive(isVisible && !isDriving);
        ui.sanityBarImages[1].gameObject.SetActive(isVisible && isDriving);

        if (!isVisible) return;

        // Sprite Update
        float percentage = currentSanity / maxSanity;
        int frameIndex = Mathf.Clamp(Mathf.FloorToInt(percentage * (sanityFrames.Length - 1)), 0, sanityFrames.Length - 1);
        
        foreach (var img in ui.sanityBarImages)
        {
            if (img != null && img.gameObject.activeInHierarchy) 
                img.sprite = sanityFrames[frameIndex];
        }
    }

    private void HandleDamageFeedback(int count)
    {
        if (ui == null || ui.damageOverlay == null) return;

        if (currentSanity < lastSanity)
        {
            float targetAlpha = Mathf.Clamp01(count * 0.25f); 
            ui.damageOverlay.alpha = Mathf.MoveTowards(ui.damageOverlay.alpha, targetAlpha, Time.deltaTime * flashSpeed);
        }
        else
        {
            ui.damageOverlay.alpha = Mathf.MoveTowards(ui.damageOverlay.alpha, 0, Time.deltaTime * (flashSpeed * 0.5f));
        }
        lastSanity = currentSanity;
    }

    private void UpdateKickButton(bool active)
    {
        if (ui != null && ui.kickOutButton != null)
            ui.kickOutButton.gameObject.SetActive(active);
    }

    public void RequestKickOut()
    {
        PassengerAI closest = null;
        float minDist = float.MaxValue;

        foreach (var p in nearbyAnomalies)
        {
            if (p != null && (p.IsBoarding || p.IsSeated))
            {
                float d = Vector3.Distance(transform.position, p.transform.position);
                if (d < minDist) { minDist = d; closest = p; }
            }
        }

        if (closest != null) closest.KickOutServerRpc();
    }

    public void RegisterAnomaly(PassengerAI anomaly) => nearbyAnomalies.Add(anomaly);
    public void UnregisterAnomaly(PassengerAI anomaly) => nearbyAnomalies.Remove(anomaly);
}