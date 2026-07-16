using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TCC.Web.InternalSystems.Services
{

    public class AppStateService : INotifyPropertyChanged
    {
        private string? _token;
        private string? _username;
        private string? _password;
        private List<Dictionary<string, string>>? _excelData;

        public string? Token
        {
            get => _token;
            set { _token = value; Notify(); }
        }

        public string? Username
        {
            get => _username;
            set { _username = value; Notify(); }
        }

        public string? Password
        {
            get => _password;
            set { _password = value; Notify(); }
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_username);

        public List<Dictionary<string, string>>? ExcelData
        {
            get => _excelData;
            set { _excelData = value; Notify(); }
        }

        public void Logout()
        {
            Token = null;
            Username = null;
            Username = null;
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
