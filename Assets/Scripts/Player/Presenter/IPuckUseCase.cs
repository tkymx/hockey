using UnityEngine;

public interface IPuckUseCase
{
    void Move(Vector2 direction, float force);
    void Grow();
    void HandleCollision(Collision collision);
}