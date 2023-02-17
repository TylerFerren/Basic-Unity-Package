using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class StatusBarUI : MonoBehaviour
{
    private Status status;
    [SerializeField] private Slider StatusBar;
    [SerializeField] private bool useStatusColor;
    [SerializeField] private bool billboard = false;


    public void Reset()
    {
        if (!StatusBar) StatusBar = GetComponent<Slider>();
        StatusBar.interactable = false;
        StatusBar.transition = Selectable.Transition.None;

        if (StatusBar.fillRect == null) {
            if (GetComponentInChildren<Image>())
                StatusBar.fillRect = GetComponentInChildren<Image>().rectTransform;
            else
                SetupStatusBar();
        }

        if(status == null) status = GetComponentInParent<Status>();
    }

    private void OnEnable()
    {
        if (status != null) status.StatusUpdate.AddListener(StatusBarUIUpdate);
    }

    private void OnDisable()
    {
        if (status != null) status.StatusUpdate.RemoveListener(StatusBarUIUpdate);
    }

    public void SetupStatusBar() {
        
        var newGameObject = new GameObject();
        newGameObject.transform.parent = transform;
        newGameObject.transform.localScale = Vector3.one;
        newGameObject.name = "FillBar";
        var image = newGameObject.AddComponent<Image>();
        StatusBar.fillRect = image.rectTransform;
        image.rectTransform.localPosition = Vector3.zero;
        StatusBar.fillRect.anchorMin = Vector2.zero;
        StatusBar.fillRect.anchorMax = new Vector2(1, 1);
        StatusBar.fillRect.anchoredPosition = Vector2.zero;
        StatusBar.fillRect.sizeDelta = Vector2.zero;
        StatusBar.fillRect.pivot = new Vector2(0.5f, 0.5f);
        StatusBar.fillRect.offsetMin = Vector2.zero;
        StatusBar.fillRect.offsetMax = Vector2.zero;
    }

    public void LateUpdate() {
        if (billboard) BillboardUI();
    }

    public void StatusBarUIUpdate(Status status) {
        StatusBar.maxValue = status.MaxValue;
        StatusBar.value = status.CurrentValue;

    }

    public void BillboardUI() {
        transform.LookAt(Camera.main.transform.position);
    }

}
