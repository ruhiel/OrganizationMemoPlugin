using Grabacr07.KanColleWrapper;
using Livet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace OrganizationMemoPlugin
{
    public class OrganizationViewModel : ViewModel
    {
        static string FilePath => "OrganizationMemo.txt";
        XmlSerializer serializer = new XmlSerializer(typeof(OrganizationFleets));

        private OrganizationFleet _DisplayFleet;
        public OrganizationFleet DisplayFleet
        {
            get
            {
                return _DisplayFleet;
            }
            set
            {
                if (value == null && SelectFleets.Count() != 0) return;
                _DisplayFleet = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsDisplayFleetNull));
            }
        }

        public Boolean IsDisplayFleetNull => DisplayFleet == null;

        public ObservableCollection<OrganizationFleet> _SelectFleets;
        public ObservableCollection<OrganizationFleet> SelectFleets
        {
            get
            {
                return _SelectFleets;
            }
            set
            {
                _SelectFleets = value;
                RaisePropertyChanged();
            }
        }

        private OrganizationFleets _OrganizationFleets;

        public OrganizationViewModel()
        {
            Init();
        }

        private void Init()
        {
            _OrganizationFleets = LoadFile();

            SelectFleets = new ObservableCollection<OrganizationFleet>(_OrganizationFleets.Fleets);
        }

        private List<OrganizationShipInfo> Fleet2OrganizationShipInfo(int i) =>
            KanColleClient.Current.Homeport.Organization.Fleets[i].Ships.Select(
                ship => new OrganizationShipInfo()
                {
                    Id = ship.Info.Id,
                    SlotIds = ship.Slots.Select(slot => slot.Item.Info.Id).ToList()
                }
            ).ToList();

        public void AddFleet(String flag)
        {
            // 連合艦隊かどうか
            bool isCombined = Convert.ToBoolean(flag);
            var newFleet = new OrganizationFleet()
            {
                FirstFleet = Fleet2OrganizationShipInfo(1),
                SecondFleet = isCombined ? Fleet2OrganizationShipInfo(2) : null,
                FirstFleetName = KanColleClient.Current.Homeport.Organization.Fleets[1].Name,
                SecondFleetName = isCombined ? KanColleClient.Current.Homeport.Organization.Fleets[2].Name : null,
                Time = DateTime.Now
            };

            _OrganizationFleets.Fleets.Add(newFleet);

            SelectFleets = new ObservableCollection<OrganizationFleet>(_OrganizationFleets.Fleets);

            DisplayFleet = newFleet;

            Task.Run(() => {
                SaveFile();
            });
        }

        private void SaveFile()
        {
            try
            {              
                //書き込むファイルを開く（UTF-8 BOM無し）
                var sw = new StreamWriter(FilePath, false, new UTF8Encoding(false));
                //シリアル化し、XMLファイルに保存する
                serializer.Serialize(sw, _OrganizationFleets);
                //ファイルを閉じる
                sw.Close();
            }
            catch (Exception)
            { 
            }
        }

        private OrganizationFleets LoadFile()
        {
            var fleets = new OrganizationFleets();
            try
            {
                //読み込むファイルを開く（UTF-8 BOM無し）
                var sr = new StreamReader(FilePath, new UTF8Encoding(false), false);
                //デシリアライズし、メンバにセットする
                fleets = serializer.Deserialize(sr) as OrganizationFleets;
                //ファイルを閉じる
                sr.Close();
            }
            catch (Exception)
            {
            }

            return fleets;
        }

        public void DeleteFleet()
        {
            if (SelectFleets.Count() == 0 || DisplayFleet == null) return;

            int index = SelectFleets
                .OrderBy(x => x.Time)
                .Select((y, i) => new { Fleet = y, Index = i })
                .Where(f => f.Fleet.Time.Equals(DisplayFleet.Time))
                .First().Index;

            OrganizationFleet fleet = SelectFleets
                .OrderBy(x => x.Time)
                .Where((d, i) => i == index - 1 || i == index + 1)
                .FirstOrDefault();

            var fleets = SelectFleets.OrderBy(x => x.Time).ToList();

            fleets.RemoveAt(index);

            _OrganizationFleets.Fleets = fleets;

            SelectFleets = new ObservableCollection<OrganizationFleet>(_OrganizationFleets.Fleets);

            DisplayFleet = fleet;

            Task.Run(
                () => {
                    SaveFile();
            });
        }
    }
}
