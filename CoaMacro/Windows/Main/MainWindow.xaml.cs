using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using WindowsInput;
using WindowsInput.Events;

namespace CoaMacro.Windows.Main;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private CancellationTokenSource? _cts;
    private int _executionCount;
    private int _coluna;
    private int _linha;
    
    // === DllImports ===
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public MainWindow()
    {
        InitializeComponent();
    }
    
    // === Botões Iniciar / Parar ===
    private async void btnStart_Click(object sender, EventArgs e)
    {
        if (_cts != null) return;

        int loops = numericLoops.Value ?? 1;
        // int loadDelay = (int)numericLoadDelay.Value;

        _cts = new CancellationTokenSource();
     
        CalcularPosicaoInicial();
        await RunMacro(loops, 6000, _cts.Token);
        
        _cts.Dispose();
        _cts = null;
    }
    
    private void CalcularPosicaoInicial()
    {
        // Coordenadas base
        int baseX = 250;
        int baseY = 100;
        int stepX = 200;
        int stepY = 200;
        int pX = numericPX.Value ?? 1;
        int pY = numericPY.Value ?? 1;
        
        _coluna = (pX == 1) ? 450 : baseX + (pX * stepX);
        _linha = (pY == 1) ? 300 : baseY + (pY * stepY);
    }
    
    private static IntPtr VerificarJanela()
    {
        string nomeParcial = "Crystal of Atlan";
        IntPtr janelaEncontrada = IntPtr.Zero;

        EnumWindows((hWnd, lParam) =>
        {
            StringBuilder sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            string titulo = sb.ToString();

            if (titulo.Contains(nomeParcial, StringComparison.OrdinalIgnoreCase))
            {
                janelaEncontrada = hWnd;
                return false; // parar
            }

            return true; // continuar
        }, IntPtr.Zero);

        if (janelaEncontrada == IntPtr.Zero)
        {
            MessageBox.Show("\u274c Erro Na Operação",
                $"Janela contendo '{nomeParcial}' não foi encontrada.\n\nDeseja fechar o sistema?");
        }

        return janelaEncontrada;
    }
    
    private async Task MoverMouseNaJanela(IntPtr hWnd, int offsetX, int offsetY)
    {
        if (hWnd == IntPtr.Zero) return;

        if (GetWindowRect(hWnd, out RECT rect))
        {
            int posX = rect.Left + offsetX;
            int posY = rect.Top + offsetY;

            await Simulate.Events()
                .MoveTo(posX, posY) // Aqui é o clique esquerdo
                .Invoke();
        }
    }
    
    private async Task RunMacro(int loops, int loadDelay, CancellationToken token)
    {
        for (int i = 0; i < loops; i++)
        {
            //if (AtualizarBotao(token)) break;

            IntPtr hWnd = VerificarJanela();
            if (hWnd == IntPtr.Zero) break;

            SetForegroundWindow(hWnd);

            UpdateCounter();
            
            await Task.Delay(50);
            await MoverMouseNaJanela(hWnd, _coluna, _linha); 
            await Task.Delay(50);
            await Simulate.Events().Click(ButtonCode.Left).Invoke();
            await Task.Delay(200);
            await MoverMouseNaJanela(hWnd, 450, 250);
            await Simulate.Events().Click(ButtonCode.Left).Invoke();
            await Task.Delay(200);
            await MoverMouseNaJanela(hWnd, 650, 570);
            await Simulate.Events().Click(ButtonCode.Left).Invoke();
            await Task.Delay(200);
            await MoverMouseNaJanela(hWnd, 995, 210);
            await Task.Delay(50);
            await Simulate.Events().Click(ButtonCode.Left).Invoke();
            await Task.Delay(50);
            await MoverMouseNaJanela(hWnd, 370, 200); 
            await Task.Delay(100);
            await Simulate.Events().Click(ButtonCode.Left).Invoke();
        }
        
    }
    
    private async void btnTeste(object sender, EventArgs e)
    {
        
        IntPtr hWnd = VerificarJanela();
        if (hWnd == IntPtr.Zero) return;

        SetForegroundWindow(hWnd);
        
        CalcularPosicaoInicial();
            
        await Task.Delay(50);
        await MoverMouseNaJanela(hWnd, _coluna, _linha); 
    }
    private void UpdateCounter()
    {
        _executionCount++;
        lblNumeroExecucoes.Text = $"Número de Execuções: {_executionCount}";
    }
}