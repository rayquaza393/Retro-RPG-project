using TMPro;
using UnityEngine;

public class NameTag : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Transform target;
    public Vector3 offset = new Vector3(0, 2.0f, 0); // height above head

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.rotation = Camera.main.transform.rotation; // always face camera
        }
    }

    public void SetName(string displayName, Transform followTarget)
    {
        nameText.text = displayName;
        target = followTarget;
    }
}
