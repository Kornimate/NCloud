namespace NCloud.Models
{
    public class Pair<T, K>
    {
        public T First { get; }
        public K Second { get; }

        public Pair(T first, K second)
        {
            First = first;
            Second = second;
        }
    }
}
