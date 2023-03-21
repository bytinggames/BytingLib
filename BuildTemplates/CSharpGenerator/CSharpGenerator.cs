using BytingLib;

namespace BuildTemplates
{
    static class CSharpGenerator
    {
        internal static XnbFolder FromMGCB(string mgcbOutput, ContentConverter contentConverter)
        {
            var actions = MGCBParser.GetMGCBActions(mgcbOutput);
            MGCBParser.Apply(actions, contentConverter);

            List<Xnb> xnbs = new List<Xnb>();
            for (int i = 0; i < actions.Count; i++)
            {
                xnbs.Add(new Xnb(actions[i].FilePath, actions[i].CSharpType!, actions[i].CSharpVarName!));
            }

            XnbFolder root = new XnbFolder("", "ContentLoader", xnbs);

            return root;
        }
    }
}
