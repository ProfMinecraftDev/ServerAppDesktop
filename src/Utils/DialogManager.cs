using System;
using System.Runtime.InteropServices;

namespace ServerAppDesktop.Utils
{
    public static class DialogManager
    {
        // Importación de MessageBoxW desde user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int MessageBoxW(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        // Resultados posibles del cuadro de diálogo
        public enum DialogResult
        {
            OK = 1,
            Cancel = 2,
            Abort = 3,
            Retry = 4,
            Ignore = 5,
            Yes = 6,
            No = 7
        }

        /// <summary>
        /// Muestra un cuadro de diálogo nativo con estilo del sistema operativo.
        /// </summary>
        /// <param name="text">Texto del mensaje.</param>
        /// <param name="caption">Título del cuadro.</param>
        /// <param name="type">Estilo del cuadro (botones e íconos).</param>
        /// <returns>Resultado del botón presionado.</returns>
        public static DialogResult Show(string text, string caption, uint type = MB_OK)
        {
            int result = MessageBoxW(IntPtr.Zero, text, caption, type);
            return Enum.IsDefined(typeof(DialogResult), result)
                ? (DialogResult)result
                : DialogResult.Cancel;
        }

        // Estilos de botones
        public const uint MB_OK = 0x00000000;
        public const uint MB_OKCANCEL = 0x00000001;
        public const uint MB_ABORTRETRYIGNORE = 0x00000002;
        public const uint MB_YESNOCANCEL = 0x00000003;
        public const uint MB_YESNO = 0x00000004;
        public const uint MB_RETRYCANCEL = 0x00000005;
        public const uint MB_CANCELTRYCONTINUE = 0x00000006;

        // Íconos
        public const uint MB_ICONERROR = 0x00000010;
        public const uint MB_ICONQUESTION = 0x00000020;
        public const uint MB_ICONWARNING = 0x00000030;
        public const uint MB_ICONINFORMATION = 0x00000040;

        // Opciones adicionales
        public const uint MB_DEFAULT_DESKTOP_ONLY = 0x00020000;
        public const uint MB_RIGHT = 0x00080000;
        public const uint MB_RTLREADING = 0x00100000;
        public const uint MB_TOPMOST = 0x00040000;
        public const uint MB_SETFOREGROUND = 0x00010000;
    }
}