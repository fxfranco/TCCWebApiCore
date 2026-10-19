using System.ComponentModel;
using System.Runtime.CompilerServices;
using TCC.Web.InternalSystems.Models;

namespace TCC.Web.InternalSystems.Services
{

    public class AppStateServiceSIM : INotifyPropertyChanged
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

        /// <summary>
        /// Estructura completa recibida de la API (RequestedFields, MissingColumns, Rows).
        /// La tabla sigue trabajando con ExcelData; esta propiedad conserva los metadatos
        /// para devolver la estructura completa al guardar (ver EnviarDatosAPI).
        /// </summary>
        public ExcelFieldsResponse? DatosApi { get; set; }

        public void Logout()
        {
            Token = null;
            ExcelData = null;
            DatosApi = null;
        }

        public void ClearData()
        {
            ExcelData = null;
            DatosApi = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Notify([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
