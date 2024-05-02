using CorpseLib.Actions;

namespace StreamGlass.Core
{
    public abstract class AStreamGlassAction(ActionDefinition actionDefinition) : AAction(actionDefinition)
    {
        public virtual bool AllowDirectCall => false;
        public virtual bool AllowCLICall => false;
        public virtual bool AllowScriptCall => false;
        public virtual bool AllowRemoteCall => false;
    }
}
