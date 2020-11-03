using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour
{
    private Text text;
    private void Awake() {
        text = GetComponent<Text>();
    }

    public void updateCount () {
        text.text = GameObject.FindObjectsOfType<SpriteRenderer>().Length.ToString();
    }
}
