namespace Rabota;

interface IInteractable
{
    Rectangle Hitbox { get; }
    void OnPlayerTouch(Player player);
}