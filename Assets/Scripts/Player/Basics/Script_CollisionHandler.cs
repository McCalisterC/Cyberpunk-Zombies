using UnityEngine;

public class Script_CollisionHandler : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy Attack" && tag == "LocalPlayer"){
            if(!other.GetComponentInParent<Script_BasicEnemy>().hasHit){
                GetComponent<Script_BaseStats>().TakeDamage(other.GetComponentInParent<Script_BasicEnemy>().damage);
                other.GetComponentInParent<Script_BasicEnemy>().hasHit = true;
            }
        }
    }
}
