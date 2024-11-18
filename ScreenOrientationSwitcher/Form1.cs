using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScreenOrientationSwitcher
{
    public partial class Form1 : Form
    {
        // Declarando o ícone da bandeja do sistema
        private NotifyIcon trayIcon;

        // Constantes para as configurações de tela
        private const int DM_PELSWIDTH = 0x80000;
        private const int DM_PELSHEIGHT = 0x100000;
        private const int DM_DISPLAYORIENTATION = 0x00000080;
        private const int DM_DO_DEFAULT = 0;
        private const int DM_DO_90 = 1;
        private const int DM_DO_180 = 2;
        private const int DM_DO_270 = 3;

        // Estrutura para obter informações sobre os dispositivos de exibição conectados
        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            public uint State;
            public uint DeviceID;
            public uint DeviceString;
        }

        // Estrutura para configurar a resolução e a orientação da tela
        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            public const int DM_SIZE = 144;
            public string dmDeviceName;
            public int dmSize;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmScreenWidth;
            public int dmScreenHeight;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFrequency;
            public int dmDisplayOrientation;
        }

        // Funções externas da API do Windows para manipular as configurações de exibição
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int EnumDisplayDevices(string deviceName, uint deviceIndex, ref DISPLAY_DEVICE displayDevice, uint flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int EnumDisplaySettings(string deviceName, uint modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int ChangeDisplaySettings(ref DEVMODE devMode, uint flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(uint uiAction, uint uiParam, ref DEVMODE lpvParam, uint fWinIni);

        // Códigos de erro para mudanças de orientação
        private const int DISP_CHANGE_FAILED = -1;
        private const int DISP_CHANGE_SUCCESSFUL = 0;

        public Form1()
        {
            InitializeComponent();
            ConfigureTrayIcon();
        }

        private void ConfigureTrayIcon()
        {
            // Criar e configurar o ícone da bandeja do sistema
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Text = "Screen Orientation Switcher",
                Visible = true
            };

            // Criando o menu de contexto para o ícone da bandeja
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Alterar para Retrato", null, (s, e) => SetOrientation(true)); // Muda para modo Retrato
            menu.Items.Add("Alterar para Paisagem", null, (s, e) => SetOrientation(false)); // Muda para modo Paisagem
            menu.Items.Add("Sair", null, (s, e) => Application.Exit()); // Sai do aplicativo
            trayIcon.ContextMenuStrip = menu;
        }

        private void SetOrientation(bool isPortrait)
        {
            // Criando um objeto para armazenar as informações do dispositivo de exibição
            DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
            displayDevice.DeviceName = new string('\0', 32);

            // Obtendo o dispositivo de exibição
            int result = EnumDisplayDevices(null, 0, ref displayDevice, 0);
            if (result == 0)
            {
                MessageBox.Show("Falha ao obter o dispositivo de exibição.");
                return;
            }

            DEVMODE devMode = new DEVMODE();
            devMode.dmSize = Marshal.SizeOf(devMode);

            // Obtendo as configurações atuais da exibição
            result = EnumDisplaySettings(displayDevice.DeviceName, 0, ref devMode);
            if (result == DISP_CHANGE_FAILED)
            {
                MessageBox.Show("Falha ao obter as configurações do monitor.");
                return;
            }

            // Ajustando a resolução e a orientação da tela
            if (isPortrait)
            {
                devMode.dmPelsWidth = 1080;
                devMode.dmPelsHeight = 1920;
                devMode.dmDisplayOrientation = DM_DO_90;  // Configura para o modo retrato (90 graus)
            }
            else
            {
                devMode.dmPelsWidth = 1920;
                devMode.dmPelsHeight = 1080;
                devMode.dmDisplayOrientation = DM_DO_DEFAULT; // Configura para o modo paisagem (padrão)
            }

            devMode.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYORIENTATION;

            // Tentando alterar as configurações da tela
            result = ChangeDisplaySettings(ref devMode, 0);
            if (result != DISP_CHANGE_SUCCESSFUL)
            {
                MessageBox.Show($"Falha ao alterar a orientação! Código de erro: {result}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // Forçando a atualização das configurações da tela
                int systemParamResult = SystemParametersInfo(0x005D, 0, ref devMode, 0x01 | 0x02);  // Força a atualização
                if (systemParamResult != DISP_CHANGE_SUCCESSFUL)
                {
                    MessageBox.Show($"Falha ao forçar a atualização da orientação! Código de erro: {systemParamResult}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Orientação alterada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
