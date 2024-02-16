
namespace StreamGlass.Core.Controls
{
    public class RadioGroup<TID>
    {
        public interface IItem
        {
            public TID GetID();
            public void OnSelected();
            public void OnUnselected();
        }

        private readonly List<IItem> m_Items = [];
        private TID? m_CurrentID = default;

        public void AddItem(IItem item)
        {
            if (EqualityComparer<TID>.Default.Equals(item.GetID(), m_CurrentID))
                item.OnSelected();
            m_Items.Add(item);
        }

        public void Select(TID id)
        {
            foreach (var item in m_Items)
            {
                if (EqualityComparer<TID>.Default.Equals(item.GetID(), id))
                    item.OnSelected();
                else
                    item.OnUnselected();
            }
            m_CurrentID = id;
        }

        public void Clear() => m_Items.Clear();
    }
}
