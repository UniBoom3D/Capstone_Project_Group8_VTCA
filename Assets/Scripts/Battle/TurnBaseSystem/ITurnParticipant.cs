public interface ITurnParticipant
{
    string Name { get; }
    int HP { get; }
    bool IsAlive { get; }

    void TakeTurn(); // hành động trong lượt
    void TakeDamage(int dmg);
}
