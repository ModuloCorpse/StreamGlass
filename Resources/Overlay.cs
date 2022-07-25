using System.Text.Json.Nodes;
using Quicksand.Web;
using Quicksand.Web.Html;

namespace StreamGlass.Resources
{
    public class Overlay : CodeDrivenHtmlResource, IBaseElementListener
    {
        private readonly List<JsonObject> m_Requests = new();

        public Overlay(): base(true) { }

        protected sealed override void OnUpdate(long deltaTime)
        {
            UpdateOverlay(deltaTime);
            SendRequests();
        }

        protected virtual void UpdateOverlay(long deltaTime) { }

        protected T NewElement<T>(string id) where T : BaseElement, new()
        {
            T element = new();
            element.SetID(id);
            element.SetListener(this);
            return element;
        }

        protected Tag NewElement(string name, string id)
        {
            Tag element = new(name);
            element.SetID(id);
            element.SetListener(this);
            return element;
        }

        protected override void Generate()
        {
            Head head = m_Page.GetHead();
            head.SetCharset("UTF-8");
            head.AddMeta(Meta.Type.VIEWPORT, "width=1920px,height=1080px");

            Script framework = new();
            framework.SetSrc("/js/woframework.js");
            head.AddScript(framework);

            Link favicon = new(Link.RelationType.ICON);
            favicon.SetType("image/x-icon");
            favicon.SetHref("/favicon.ico");
            head.AddLink(favicon);

            GenerateOverlay();
        }

        protected virtual void GenerateOverlay() { }

        private void AddRequest(JsonObject obj)
        {
            m_Requests.Add(obj);
        }

        public void SendRequests()
        {
            if (m_Requests.Count > 0)
            {
                JsonObject requests = new();
                JsonArray requestArray = new();
                foreach (JsonObject request in m_Requests)
                    requestArray.Add(request);
                requests["requests"] = requestArray;
                m_Requests.Clear();
                Send(requests.ToJsonString());
            }
        }

        public void OnContentAdded(BaseElement element, string content)
        {
            //throw new NotImplementedException();
        }

        public void OnContentRemoved(BaseElement element, string content)
        {
            //throw new NotImplementedException();
        }

        public void OnChildAdded(BaseElement element, BaseElement child)
        {
            //throw new NotImplementedException();
        }

        public void OnChildRemoved(BaseElement element, BaseElement child)
        {
            //throw new NotImplementedException();
        }

        public void OnAttributeAdded(BaseElement element, string attribute, object value)
        {
            string? valueStr = "";
            if (value is bool tmp)
            {
                if (!tmp)
                {
                    OnAttributeRemoved(element, attribute);
                    return;
                }
            }
            else
                valueStr = value.ToString();
            AddRequest(new()
            {
                ["name"] = "attribute-added",
                ["id"] = (string)element["id"],
                ["attribute"] = attribute,
                ["value"] = valueStr
            });
        }

        public void OnAttributeRemoved(BaseElement element, string attribute)
        {
            AddRequest(new()
            {
                ["name"] = "attribute-removed",
                ["id"] = (string)element["id"],
                ["attribute"] = attribute
            });
        }

        public void OnRegistered(BaseElement element)
        {
            if (string.IsNullOrWhiteSpace(element.GetID()))
                element.SetListener(null);
        }

        public void OnUnregistered(BaseElement element) {}
    }
}
