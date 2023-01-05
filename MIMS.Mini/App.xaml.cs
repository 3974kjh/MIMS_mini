using MIMS.Mini.Defines; // MIMSMiniDefines
using MIMS.Mini.Foundation;
using MIMS.Mini.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MIMS.Mini
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private LogListBox _logger;

        public App()
        {
            this.Engine = new MIMSClientEngine();
        }

        public MIMSClientEngine Engine { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                //로그 초기화
                var appDataFolderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), MIMSMiniDefines.PROGRAM_NAME);

                if (false == System.IO.Directory.Exists(appDataFolderPath))
                    System.IO.Directory.CreateDirectory(appDataFolderPath);

                _logger = new LogListBox();
                _logger.Init(System.IO.Path.Combine(appDataFolderPath, $"{MIMSMiniDefines.LOG_FOLDER_NAME}/{MIMSMiniDefines.LOG_FILE_NAME}"));
                SimpleLogger.SetLogger(_logger);

                SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, $"{MIMSMiniDefines.PROGRAM_NAME}을 시작합니다.");

                //엔진 초기화
                if (false == this.Engine.Init())
                {             
                    SimpleLogger.Instance()._OutputErrorMsg("[MIMS 엔진 초기화를 실패하였습니다.");
                    this.Shutdown();
                    return;
                }
            }
            catch (Exception ex)
            {
                var message = new System.Text.StringBuilder();
                message.Append("프로그램 초기화 오류가 발생했습니다.\n");
                message.Append(ex.Message);
                message.Append("프로그램을 종료합니다.\n");

                SimpleLogger.Instance()._OutputErrorMsg(message.ToString());

                MessageBox.Show(Application.Current.MainWindow, message.ToString(), "초기화 오류", MessageBoxButton.OK, MessageBoxImage.Error);

                this.Shutdown();
            }

            //프로그램 내에서 처리하지 않은 예외 처리 이벤트 연결
            Application.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(OnDispatcherUnhandledException);

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //엔진 종료
            this.Engine.Term();

            //로그 종료
            SimpleLogger.Instance()._OutputMsg(SimpleLogger.LOG_LEVEL.INFO, $"{MIMSMiniDefines.PROGRAM_NAME}을 종료합니다.");
            SimpleLogger.Instance().Terminate();

            //프로그램 내에서 처리하지 않은 예외 처리 이벤트 해제
            Application.Current.DispatcherUnhandledException -= new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(OnDispatcherUnhandledException);

            DirectoryInfo dirlnfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "PatientThumbnailImage");

            foreach (var file in dirlnfo.GetFiles())
            {
                file.Delete();
            }

            base.OnExit(e);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Exception.Message);
            e.Handled = true;

            var sb = new System.Text.StringBuilder();
            sb.Append("프로그램을 진행하는 도중 알 수 없는 오류가 발생하였습니다.\n");
            sb.Append("프로그램 관리자나 제조사로 문의하시기 바랍니다.\n 프로그램이 종료됩니다.");
            sb.Append("\n- 오류 : ");
            sb.Append(e.Exception.Message);

            if (null != this.MainWindow && true == this.MainWindow.IsActive)
            {
                MessageBox.Show(this.MainWindow, sb.ToString(), "알 수 없는 프로그램 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(sb.ToString(), "알 수 없는  프로그램 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            SimpleLogger.Instance()._OutputErrorMsg(sb.ToString());

            this.Shutdown();
        }
    }
}
