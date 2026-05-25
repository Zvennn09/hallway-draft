using UnityEngine;

public class CreditScript : MonoBehaviour
{
    public float scrollSpeed = 35f;
    public float startDelay = 5f;
    public float stopY = 1859f;

    private RectTransform rectTransform;
    private float delayTimer;
    private bool hasStopped = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        delayTimer = Mathf.Max(0f, startDelay);
    }

    void Update()
    {
        if (hasStopped) return;

        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        if (rectTransform.anchoredPosition.y >= stopY)
        {
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, stopY);
            hasStopped = true;
        }
    }
}