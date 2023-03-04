using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

namespace Codesign
{
    public class StatusTextUI : MonoBehaviour
    {
        [SerializeField] private Status status;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField, ReadOnly] private string statusText; 
        [SerializeField] private bool billboard = false;

        private void OnValidate()
        {
            if (status) StatusBarUIUpdate(status);
            if (text == null) text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (status != null) status.StatusUpdate.AddListener(StatusBarUIUpdate);
        }

        private void OnDisable()
        {
            if (status != null) status.StatusUpdate.RemoveListener(StatusBarUIUpdate);
        }

        public void StatusBarUIUpdate(Status status)
        {
            statusText = status.CurrentValue.ToString() + "/" + status.MaxValue.ToString();
            text.text = statusText;
        }

        public void LateUpdate()
        {
            if (billboard) BillboardUI();
        }


        public void BillboardUI()
        {
            transform.LookAt(Camera.main.transform.position);
        }

    }
}