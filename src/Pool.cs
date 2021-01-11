using System;

namespace FosterPlatformer
{
    public abstract class Poolable
    {
        public Poolable Next = null;
        public Poolable Prev = null;
    }

    public class Pool<T> where T : Poolable
    {
        public T First;
        public T Last;

        public void Insert(T instance)
        {
            if (Last != null) {
                Last.Next = instance;
                instance.Prev = Last;
                instance.Next = null;
                Last = instance;
            }
            else {
                First = Last = instance;
                instance.Prev = instance.Next = null;
            }
        }

        public void Remove(T instance)
        {
            if (instance.Prev != null)
                instance.Prev.Next = instance.Next;
            if (instance.Next != null)
                instance.Next.Prev = instance.Prev;

            if (First == instance)
                First = (T) instance.Next;
            if (Last == instance)
                Last = (T) instance.Prev;

            instance.Next = null;
            instance.Prev = null;
        }
    }
}
