using UnityEngine;

public interface ITurnParticipant
{
    string Name { get; }
    float HP { get; }
    bool IsAlive { get; }
    //Transform transform { get; set; } 

    void TakeTurn(); // hành động trong lượt
  
    void TakeDamage(float damage);
}
