using System;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using CentralMonitorGUI.Properties;
using CentralMonitorGUI.ViewModels;
using CentralMonitorGUI.Views;
using NetworkCommunication;
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
            var newRangeThreshold = TimeSpan.FromMinutes(5);
            var appendToFile = true;
            var dataDirectory = Settings.Default.DataPath;
            if (string.IsNullOrEmpty(dataDirectory))
                dataDirectory = "Data";
            var fileManager = new FileManager(dataDirectory);

            var mainCancellationTokenSource = new CancellationTokenSource();
            var discoveryMessageParser = new DiscoveryMessageParser();
            var discoveryMessageReceiver = new DiscoveryMessageReceiver(discoveryMessageParser);
            var network = new MonitorNetwork(discoveryMessageReceiver, connectionLostTimeout);
            var dataRequestGenerator = new DataRequestGenerator();
            var vitalSignsUdpClient = new UdpClient(Informations.VitalSignsRequestOutboundPort);
            var waveformUdpClient = new UdpClient(Informations.WaveformRequestOutboundPort);
            var dataRequestSender = new DataRequestSender(dataRequestGenerator, vitalSignsUdpClient, waveformUdpClient);
            var vitalSignPacketParser = new VitalSignPacketParser();
            var waveformPacketParser = new WaveformPacketParser();
            var waveformAndVitalSignReceiver = new WaveformAndVitalSignReceiver(
                vitalSignPacketParser,
                waveformPacketParser,
                vitalSignsUdpClient,
                waveformUdpClient);
            var alarmMessageParser = new AlarmMessageParser();
            var alarmReceiver = new AlarmReceiver(alarmMessageParser);
            var dataConnectionManager = new DataConnectionManager(network, waveformAndVitalSignReceiver, dataRequestSender, alarmReceiver);
            var updateTrigger = new UpdateTrigger(waveformUpdateInterval);
            var availableDataFinder = new AvailableDataFinder(fileManager, newRangeThreshold);
            var vitalSignFileLoader = new VitalSignFileLoader(fileManager);
            var waveformFileLoader = new WaveformFileLoader(fileManager);
            var historyLoader = new HistoryLoader(fileManager, availableDataFinder, vitalSignFileLoader, waveformFileLoader);
            var dataExplorerWindowViewModelFactory = new DataExplorerWindowViewModelFactory(historyLoader, fileManager);
            var mainWindow = new MainWindow();
            mainWindow.ViewModel = new MainViewModel(network, updateTrigger, fileManager, dataExplorerWindowViewModelFactory, mainWindow.Close);

            using (var waveformStorer = new WaveformStorer(network, fileManager, appendToFile))
            using (var vitalSignsStorer = new VitalSignsStorer(network, fileManager, appendToFile))
            {
                waveformAndVitalSignReceiver.NewWaveformData += (receiver, data) => waveformStorer.Store(data);
                waveformAndVitalSignReceiver.NewVitalSignData += (receiver, data) => vitalSignsStorer.Store(data);
                waveformAndVitalSignReceiver.StartReceiving(mainCancellationTokenSource.Token);
                alarmReceiver.StartReceiving(mainCancellationTokenSource.Token);
                discoveryMessageReceiver.StartReceiving(mainCancellationTokenSource.Token);
                updateTrigger.Start();

                mainWindow.ShowDialog();

                // Shutdown
                updateTrigger.Stop();
                dataConnectionManager.Dispose();
                mainCancellationTokenSource.Cancel();
                waveformAndVitalSignReceiver.Dispose();
                vitalSignsUdpClient.Dispose();
                waveformUdpClient.Dispose();
            }
        }
    }
}
