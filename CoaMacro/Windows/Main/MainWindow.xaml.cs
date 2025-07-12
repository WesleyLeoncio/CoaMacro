using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using CoaMacro.Windows.Main.Struct;
using WindowsInput;
using WindowsInput.Events;

namespace CoaMacro.Windows.Main
{
    public partial class MainWindow 
    {
        private CancellationTokenSource? _cts;
        private int _executionCount;
        private int _coluna;
        private int _linha;
        private const int WmHotkey = 0x0312;
        private const int HotkeyId = 9000;

        // === DLL Imports ===
        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")] private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RectPerson lpRectPerson);
        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public MainWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += (_, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    _cts?.Cancel();
                }
            };
        }
        
     
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;
            var source = HwndSource.FromHwnd(hwnd);
            if (source != null) source.AddHook(HwndHook);

            // ESC = 0x1B
            RegisterHotKey(hwnd, HotkeyId, 0, 0x1B);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
            {
                _cts?.Cancel();
                MessageBox.Show("Macro interrompida via tecla ESC", "Cancelado", MessageBoxButton.OK, MessageBoxImage.Warning);
                handled = true;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(hwnd, HotkeyId);
            base.OnClosed(e);
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            if (_cts != null) return;

            int loops = NumericLoops.Value ?? 1;
            int delay = NumericDelay.Value ?? 1000;
            _cts = new CancellationTokenSource();

            CalcularPosicaoInicial();

            try
            {
                await RunMacro(loops, delay);
            }
            catch (OperationCanceledException)
            {
                // Cancelado
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
            }
        }

        private void CalcularPosicaoInicial()
        {
            int baseX = 250, baseY = 100, stepX = 200, stepY = 200;
            int pX = NumericPx.Value ?? 1;
            int pY = NumericPy.Value ?? 1;
            _coluna = (pX == 1) ? 450 : baseX + (pX * stepX);
            _linha = (pY == 1) ? 300 : baseY + (pY * stepY);
        }

        private static IntPtr VerificarJanela()
        {
            string nomeParcial = "Crystal of Atlan";
            IntPtr janelaEncontrada = IntPtr.Zero;

            EnumWindows((hWnd, _) =>
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                if (sb.ToString().Contains(nomeParcial, StringComparison.OrdinalIgnoreCase))
                {
                    janelaEncontrada = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            if (janelaEncontrada == IntPtr.Zero)
            {
                MessageBox.Show("\u274c Erro Na Operação",
                    $"Janela contendo '{nomeParcial}' não foi encontrada.\n\nDeseja fechar o sistema?");
            }

            return janelaEncontrada;
        }

        private static async Task MoverMouseNaJanela(IntPtr hWnd, int offsetX, int offsetY)
        {
            if (hWnd == IntPtr.Zero) return;

            if (GetWindowRect(hWnd, out RectPerson rect))
            {
                int posX = rect.Left + offsetX;
                int posY = rect.Top + offsetY;
                await Simulate.Events().MoveTo(posX, posY).Invoke();
            }
        }

        private async Task RunMacro(int loops, int delay)
        {
            var token = _cts?.Token ?? CancellationToken.None;

            for (int i = 0; i < loops; i++)
            {
                if (token.IsCancellationRequested) break;

                IntPtr hWnd = VerificarJanela();
                if (hWnd == IntPtr.Zero) break;

                SetForegroundWindow(hWnd);
                UpdateCounter();

                await GerarDelay(50, token);
                await MoverMouseNaJanela(hWnd, _coluna, _linha);
                await GerarDelay(50, token);
                await Simulate.Events().Click(ButtonCode.Left).Invoke();
                await GerarDelay(200, token);
                await MoverMouseNaJanela(hWnd, 450, 250);
                await Simulate.Events().Click(ButtonCode.Left).Invoke();
                await GerarDelay(200, token);
                await MoverMouseNaJanela(hWnd, 650, 570);
                await Simulate.Events().Click(ButtonCode.Left).Invoke();
                await GerarDelay(200, token);
                await MoverMouseNaJanela(hWnd, 995, 210);
                await GerarDelay(50, token);
                await Simulate.Events().Click(ButtonCode.Left).Invoke();
                await GerarDelay(50, token);
                await MoverMouseNaJanela(hWnd, 370, 200);
                await GerarDelay(100, token);
                await Simulate.Events().Click(ButtonCode.Left).Invoke();
                await GerarDelay(delay, token);
            }
        }

        private async void BtnTeste(object sender, EventArgs e)
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
            LblNumeroExecucoes.Text = $"Número de Execuções: {_executionCount}";
        }

        private static async Task GerarDelay(int tempo, CancellationToken token)
        {
            if (tempo <= 0) tempo = 50;
            await Task.Delay(tempo, token);
        }
    }
}
