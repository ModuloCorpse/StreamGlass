using Quicksand.Web;
using Quicksand.Web.Http;

namespace StreamGlass.Resources
{
    public class Index: Resource
    {
        protected override void Get(int clientID, Request request)
        {
            SendResponse(clientID, Defines.NewResponse(request.Version, 200, "<p>Hello World !</p>"));
        }
    }
}
