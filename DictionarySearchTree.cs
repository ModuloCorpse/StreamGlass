using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamGlass
{
    public class DictionarySearchTree<T>
    {
        private class DictionarySearchTreeNode<U>
        {
            private readonly DictionarySearchTreeNode<U>?[] m_Children = Enumerable.Repeat<DictionarySearchTreeNode<U>?>(null, char.MaxValue).ToArray();
            private bool m_HasValue = false;
            private U? m_Value = default;
            private ulong m_MaxDepth = 0;

            internal void SetValue(U value)
            {
                m_Value = value;
                m_HasValue = true;
            }

            internal void Add(string path, U value)
            {
                DictionarySearchTreeNode<U>? dictionarySearchTreeNode = m_Children[path[0]];
                if (dictionarySearchTreeNode == null)
                {
                    dictionarySearchTreeNode = new();
                    m_Children[path[0]] = dictionarySearchTreeNode;
                }
                if (path.Length > 1)
                    dictionarySearchTreeNode.Add(path[1..], value);
                else
                    dictionarySearchTreeNode.SetValue(value);
                ulong childDepth = dictionarySearchTreeNode.m_MaxDepth;
                if ((childDepth + 1) > m_MaxDepth)
                    m_MaxDepth = childDepth + 1;
            }

            internal void Fill(ref List<U> ret)
            {
                if (m_HasValue && m_Value != null)
                    ret.Add(m_Value);
                foreach (DictionarySearchTreeNode<U>? child in m_Children)
                {
                    if (child != null)
                        child.Fill(ref ret);
                }
            }

            internal void Get(string path, ref List<U> ret)
            {
                DictionarySearchTreeNode<U>? dictionarySearchTreeNode = m_Children[path[0]];
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
                foreach (DictionarySearchTreeNode<U>? child in m_Children)
                {
                    if (child != null)
                        child.Search(path, ref ret);
                }
            }
        }

        private readonly DictionarySearchTreeNode<T> m_Root = new();

        public void Add(string path, T value) => m_Root.Add(path, value);

        public List<T> Get(string path)
        {
            List<T> ret = new();
            m_Root.Get(path, ref ret);
            return ret;
        }

        public List<T> Search(string path)
        {
            List<T> ret = new();
            m_Root.Search(path, ref ret);
            return ret;
        }
    }
}
