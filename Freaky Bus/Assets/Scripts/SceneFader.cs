using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image fadePanel; // Assign your black UI Image
    [SerializeField] private float fadeDuration = 1f; // seconds

    private void Start()
    {
        // Optional: fade in at scene start
        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 1f; // start fully black
            fadePanel.color = c;
            StartCoroutine(FadeIn());
        }
    }

    // ---------------- PUBLIC METHODS ----------------

    /// <summary>
    /// Fade to a scene by name
    /// </summary>
    public void FadeToScene(string sceneName)
    {
        if (fadePanel == null) return;
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    /// <summary>
    /// Fade out only (optional, for menus)
    /// </summary>
    public void FadeOutOnly(System.Action onComplete = null)
    {
        if (fadePanel == null) return;
        StartCoroutine(FadeOutCoroutine(onComplete));
    }

    // ---------------- PRIVATE COROUTINES ----------------

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color c = fadePanel.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            fadePanel.color = c;
            yield return null;
        }

        // Ensure fully transparent at end
        c.a = 0f;
        fadePanel.color = c;
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return FadeOutCoroutine();
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOutCoroutine(System.Action onComplete = null)
    {
        float elapsed = 0f;
        Color c = fadePanel.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        // Ensure fully black at end
        c.a = 1f;
        fadePanel.color = c;

        onComplete?.Invoke();
    }
}
