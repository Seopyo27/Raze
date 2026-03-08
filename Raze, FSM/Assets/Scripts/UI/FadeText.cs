using System;
using UnityEngine;

public class FadeText : MonoBehaviour
{
    public Action OnDestroyed;
    public void OnDestroy() {
        OnDestroyed?.Invoke();
        Destroy(gameObject);
    }
}
