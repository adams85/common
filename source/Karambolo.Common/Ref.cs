namespace Karambolo.Common
{
    public static class Ref
    {
        public static Ref<T> Create<T>(T value)
        {
            return new Ref<T>(value);
        }
    }

    public class Ref<T>
    {
        public Ref(T value)
        {
            Value = value;
        }

        public T Value;
    }
}
