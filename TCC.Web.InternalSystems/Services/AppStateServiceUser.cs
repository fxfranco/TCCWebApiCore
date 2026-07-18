using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TCC.Web.InternalSystems.Services
{
    public class AppStateServiceUser : INotifyPropertyChanged
    {
        private string? _username;
        private string? _password;

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


        public void Logout()
        {
            Username = null;
            Username = null;
        }

        public void ClearData()
        {
            //ExcelData = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
