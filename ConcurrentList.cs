using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamGlass
{
    public class ConcurrentList<T>
    {
        public delegate void IterateDel(T item);

        private readonly List<T> m_List = new();
        private readonly object m_Lock = new();
        private readonly dynamic m_Id;

        public ConcurrentList(dynamic id) => m_Id = id;

        public void Add(T obj)
        {
            Logger.Log("Concurrent List", string.Format("Start adding on {0}", m_Id));
            lock (m_Lock) { m_List.Add(obj); }
            Logger.Log("Concurrent List", string.Format("Stop adding on {0}", m_Id));
        }
        public void Remove(T obj)
        {
            Logger.Log("Concurrent List", string.Format("Start removing on {0}", m_Id));
            lock (m_Lock) { m_List.Remove(obj); }
            Logger.Log("Concurrent List", string.Format("Stop removing on {0}", m_Id));
        }
        public void Iterate(IterateDel del)
        {
            Logger.Log("Concurrent List", string.Format("Start iterate on {0}", m_Id));
            lock (m_Lock) { foreach (T item in m_List) del(item); }
            Logger.Log("Concurrent List", string.Format("Stop iterate on {0}", m_Id));
        }
        public void AsyncIterate(IterateDel del) { Task.Run(() => { Iterate(del); }); }
    }
}
