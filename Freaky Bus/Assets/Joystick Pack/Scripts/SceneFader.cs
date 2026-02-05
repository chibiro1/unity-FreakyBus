using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 1f; 

    private void Start()
    {

        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 1f;
            fadePanel.color = c;
            StartCoroutine(FadeIn());
        }
    }

    // ---------------- PUBLIC METHODS ----------------
    public void FadeToScene(string sceneName)
    {
        if (fadePanel == null) return;
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

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

        c.a = 1f;
        fadePanel.color = c;

        onComplete?.Invoke();
    }
}
