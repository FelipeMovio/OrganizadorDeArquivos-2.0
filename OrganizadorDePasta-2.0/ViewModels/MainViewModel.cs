using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OrganizadorDePasta_2._0.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private string _pastaMonitorada = string.Empty;

    private string _status =
        "🔴 Monitoramento parado";

    private int _arquivosOrganizados;

    private string _mensagem =
        "Nenhuma mensagem";

    public string PastaMonitorada
    {
        get => _pastaMonitorada;
        set
        {
            if (_pastaMonitorada == value)
            {
                return;
            }

            _pastaMonitorada = value;
            OnPropertyChanged();
        }
    }

    public string Status
    {
        get => _status;
        set
        {
            if (_status == value)
            {
                return;
            }

            _status = value;
            OnPropertyChanged();
        }
    }

    public int ArquivosOrganizados
    {
        get => _arquivosOrganizados;
        set
        {
            if (_arquivosOrganizados == value)
            {
                return;
            }

            _arquivosOrganizados = value;
            OnPropertyChanged();
        }
    }

    public string Mensagem
    {
        get => _mensagem;
        set
        {
            if (_mensagem == value)
            {
                return;
            }

            _mensagem = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(
        [CallerMemberName] string? nomePropriedade = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(nomePropriedade));
    }
}