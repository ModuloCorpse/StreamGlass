using Quicksand.Web.Html;

namespace StreamGlass.Resources
{
    public class OverlayHtmlElementListener : IBaseElementListener
    {
        private readonly string m_ID;
        public OverlayHtmlElementListener(string id, BaseElement element)
        {
            m_ID = id;
            element.SetID(id);
            element.SetListener(this);
        }

        public void OnAttributeAdded(BaseElement element, string attribute, object value)
        {
            throw new NotImplementedException();
        }

        public void OnAttributeRemoved(BaseElement element, string attribute)
        {
            throw new NotImplementedException();
        }

        public void OnChildAdded(BaseElement element, BaseElement child)
        {
            throw new NotImplementedException();
        }

        public void OnChildRemoved(BaseElement element, BaseElement child)
        {
            throw new NotImplementedException();
        }

        public void OnContentAdded(BaseElement element, string content)
        {
            throw new NotImplementedException();
        }

        public void OnContentRemoved(BaseElement element, string content)
        {
            throw new NotImplementedException();
        }

        public void OnRegistered(BaseElement element) {}

        public void OnUnregistered(BaseElement element) {}
    }
}
