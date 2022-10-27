using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamGlass
{
    public class ConcurrentList<T>
    {
        public delegate void IterateDel(T item);

        private readonly List<T> m_List = new();
        private readonly object m_Lock = new();

        public void Add(T obj) { lock (m_Lock) { m_List.Add(obj); } }
        public void Remove(T obj) { lock (m_Lock) { m_List.Remove(obj); } }
        public void Iterate(IterateDel del) { lock (m_Lock) { foreach (T item in m_List) del(item); } }
        public void AsyncIterate(IterateDel del) { Task.Run(() => { Iterate(del); }); }
    }
}
