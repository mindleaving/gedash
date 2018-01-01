using System;
using System.Threading;
using System.Windows;
using CentralMonitorGUI.ViewModels;
using CentralMonitorGUI.Views;
using NetworkCommunication.Communicators;
using NetworkCommunication.DataProcessing;
using NetworkCommunication.DataStorage;
using NetworkCommunication.Objects;

namespace CentralMonitorGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var connectionLostTimeout = TimeSpan.FromSeconds(30);
            var waveformUpdateInterval = TimeSpan.FromMilliseconds(33);

            var discoveryMessageParser = new DiscoveryMessageParser();
            var discoveryMessageReceiver = new DiscoveryMessageReceiver(discoveryMessageParser);
            var network = new MonitorNetwork(discoveryMessageReceiver, connectionLostTimeout);
            var dataRequestGenerator = new DataRequestGenerator();
            var dataRequestSender = new DataRequestSender(dataRequestGenerator);
            var vitalSignPacketParser = new VitalSignPacketParser();
            var waveformPacketParser = new WaveformPacketParser();
            var waveformAndVitalSignReceiver = new WaveformAndVitalSignReceiver(dataRequestSender, vitalSignPacketParser, waveformPacketParser);
            var alarmMessageParser = new AlarmMessageParser();
            var alarmReceiver = new AlarmReceiver(alarmMessageParser);
            var dataConnectionManager = new DataConnectionManager(network, waveformAndVitalSignReceiver, alarmReceiver);
            var updateTrigger = new UpdateTrigger(waveformUpdateInterval);
            var mainViewModel = new MainViewModel(network, dataConnectionManager, updateTrigger);
            var mainWindow = new MainWindow(mainViewModel);

            var appendToFile = true;
            var directory = $@"C:\Temp";
            var mainCancellationTokenSource = new CancellationTokenSource();
            var fileManager = new FileManager(directory);
            using (var waveformStorer = new WaveformStorer(network, fileManager, appendToFile))
            using (var vitalSignsStorer = new VitalSignsStorer(network, fileManager, appendToFile))
            {
                waveformAndVitalSignReceiver.NewWaveformData += (receiver, data) => waveformStorer.Store(data);
                waveformAndVitalSignReceiver.NewVitalSignData += (receiver, data) => vitalSignsStorer.Store(data);
                discoveryMessageReceiver.StartReceiving(mainCancellationTokenSource.Token);
                alarmReceiver.StartReceiving(mainCancellationTokenSource.Token);
                updateTrigger.Start();

                mainWindow.ShowDialog();

                // Shutdown
                updateTrigger.Stop();
                dataConnectionManager.Dispose();
                mainCancellationTokenSource.Cancel();
            }
        }
    }
}
