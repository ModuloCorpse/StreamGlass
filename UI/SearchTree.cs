using Newtonsoft.Json.Linq;
using Quicksand.Web.Js;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StreamGlass.ManagedObject
{
    public class SearchTree<T>
    {
        private class SearchTreeNode<U>
        {
            private readonly Dictionary<char, SearchTreeNode<U>> m_Children = new();
            private bool m_HasValue = false;
            private U? m_Value = default;
            private ulong m_MaxDepth = 0;

            internal void Reset()
            {
                m_Children.Clear();
                m_HasValue = false;
                m_Value = default;
                m_MaxDepth = 0;
            }

            private SearchTreeNode<U>? GetChild(char c)
            {
                if (m_Children.TryGetValue(c, out var ret))
                    return ret;
                return null;
            }

            private void SetValue(U value)
            {
                m_Value = value;
                m_HasValue = true;
            }

            internal bool HaveValue()
            {
                if (m_HasValue)
                    return true;
                foreach (SearchTreeNode<U> child in m_Children.Values)
                {
                    if (child.HaveValue())
                        return true;
                }
                return false;
            }

            internal void RecomputeMaxDepth(string path)
            {
                m_MaxDepth = 0;
                SearchTreeNode<U>? dictionarySearchTreeNode = GetChild(path[0]);
                if (path.Length > 1)
                    dictionarySearchTreeNode?.RecomputeMaxDepth(path[1..]);
                foreach (SearchTreeNode<U> child in m_Children.Values)
                {
                    ulong childDepth = child.m_MaxDepth + 1;
                    if (childDepth > m_MaxDepth)
                        m_MaxDepth = childDepth;
                }
            }

            private void CleanNode()
            {
                List<char> keyToRemove = new();
                foreach (var pair in m_Children)
                {
                    if (!pair.Value.HaveValue())
                        keyToRemove.Add(pair.Key);
                }
                foreach (char key in keyToRemove)
                    m_Children.Remove(key);
            }

            internal void Remove(string path)
            {
                SearchTreeNode<U>? dictionarySearchTreeNode = GetChild(path[0]);
                if (dictionarySearchTreeNode == null)
                    return;
                if (path.Length > 1)
                    dictionarySearchTreeNode.Remove(path[1..]);
                else
                {
                    dictionarySearchTreeNode.m_Value = default;
                    dictionarySearchTreeNode.m_HasValue = false;
                }
                CleanNode();
            }

            internal void Add(string path, U value)
            {
                SearchTreeNode<U>? dictionarySearchTreeNode = GetChild(path[0]);
                if (dictionarySearchTreeNode == null)
                {
                    dictionarySearchTreeNode = new();
                    m_Children[path[0]] = dictionarySearchTreeNode;
                }
                if (path.Length > 1)
                    dictionarySearchTreeNode.Add(path[1..], value);
                else
                    dictionarySearchTreeNode.SetValue(value);
            }

            internal void Fill(ref List<U> ret)
            {
                if (m_HasValue && m_Value != null)
                    ret.Add(m_Value);
                foreach (SearchTreeNode<U> child in m_Children.Values)
                    child.Fill(ref ret);
            }

            internal void Get(string path, ref List<U> ret)
            {
                SearchTreeNode<U>? dictionarySearchTreeNode = GetChild(path[0]);
                if (dictionarySearchTreeNode == null)
                    return;
                if (path.Length > 1)
                {
                    if ((ulong)path.Length < (m_MaxDepth + 1))
                        dictionarySearchTreeNode.Get(path[1..], ref ret);
                }
                else
                    dictionarySearchTreeNode.Fill(ref ret);
            }

            internal void Search(string path, ref List<U> ret)
            {
                if ((ulong)path.Length > (m_MaxDepth + 1))
                    return;
                Get(path, ref ret);
                foreach (SearchTreeNode<U> child in m_Children.Values)
                    child.Search(path, ref ret);
            }
        }

        private readonly SearchTreeNode<T> m_Root = new();

        public void Clear() => m_Root.Reset();

        public void Add(string path, T value)
        {
            if (path.Length == 0)
                return;
            m_Root.Add(path, value);
            m_Root.RecomputeMaxDepth(path);
        }

        public void Remove(string path)
        {
            if (path.Length == 0)
                return;
            m_Root.Remove(path);
            m_Root.RecomputeMaxDepth(path);
        }

        public List<T> Get(string path)
        {
            List<T> ret = new();
            if (path.Length == 0)
                m_Root.Fill(ref ret);
            else
                m_Root.Get(path, ref ret);
            return ret;
        }

        public List<T> Search(string path)
        {
            List<T> ret = new();
            if (path.Length == 0)
                m_Root.Fill(ref ret);
            else
                m_Root.Search(path, ref ret);
            return ret;
        }
    }
}
