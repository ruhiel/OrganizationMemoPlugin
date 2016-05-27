using Grabacr07.KanColleViewer.Composition;
using System.ComponentModel.Composition;

namespace OrganizationMemoPlugin
{
    [Export(typeof(IPlugin))]
    [ExportMetadata("Guid", "7100062E-9D96-40F6-A06B-4DAA850B472C")]
    [ExportMetadata("Title", "OrganizationMemo")]
    [ExportMetadata("Description", "艦隊編成メモ一覧を表示します。")]
    [ExportMetadata("Version", "0.0.1")]
    [ExportMetadata("Author", "@ruhiel_murrue")]
    [Export(typeof(ITool))]
    public class OrganizationMemoPlugin : ITool, IPlugin
    {
        public string Name => "OrganizationMemo";

        public object View => new UserControl1 { DataContext = new OrganizationViewModel() };

        public void Initialize()
        {
        }
    }
}
