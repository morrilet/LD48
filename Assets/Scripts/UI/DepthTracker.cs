using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DepthTracker : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI localText;

    private void Awake() {
        if (localText == null)
            localText = this.GetComponent<TextMeshProUGUI>();
    }

    private void Update() {
        localText.SetText($"{GameManager.instance.FormattedDepth()} m");
    }
}
