
using UnityEngine;

[AddComponentMenu("AR Health")]
public class Health : MonoBehaviour
{
    public int health;

    public Health()
    {
        health = 5;
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetHealth(int _health)
    {
        health = _health;
    }

    public void Hit()
    {
        health--;
    }
}
