using UnityEngine;

public interface ITurnParticipant
{
    string Name { get; }
    int HP { get; }
    bool IsAlive { get; }
    Transform transform { get; set; } 

    void TakeTurn(); // hành động trong lượt
    void TakeDamage(int dmg);
}
