using UnityEngine;

public class Script_FloatingDamage : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindGameObjectWithTag("LocalPlayer") != null)
            gameObject.transform.rotation = Quaternion.RotateTowards(transform.rotation, GameObject.FindGameObjectWithTag("LocalPlayer").transform.rotation, 360f);
    }
}
