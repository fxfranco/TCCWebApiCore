using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TCC.Web.InternalSystems.Services
{

    public class AppStateServiceOpenComex : INotifyPropertyChanged
    {
        private string? _token;
        private List<Dictionary<string, string>>? _excelData;

        public string? Token
        {
            get => _token;
            set { _token = value; Notify(); }
        }

        public List<Dictionary<string, string>>? ExcelData
        {
            get => _excelData;
            set { _excelData = value; Notify(); }
        }

        public void Logout()
        {
            Token = null;
            ExcelData = null;
        }

        public void ClearData()
        {
            ExcelData = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
