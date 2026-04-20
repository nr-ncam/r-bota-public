namespace Rabota;

interface IValueTracker<T>
{
    event Action<T> ValueChanged;
}