﻿using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkCommunication
{
    public class VitalSignDataStreamer : IDisposable
    {
        readonly VitalSignMessageBuilder messageBuilder;
        readonly TimeSpan streamingTimeout = TimeSpan.FromSeconds(30);
        readonly ConcurrentDictionary<IPAddress, StoppableTask> streamingTasks = new ConcurrentDictionary<IPAddress, StoppableTask>();
        readonly ConcurrentDictionary<IPAddress, DateTime> lastRequestTimestamp = new ConcurrentDictionary<IPAddress, DateTime>();

        public VitalSignDataStreamer(VitalSignMessageBuilder vitalSignMessageBuilder)
        {
            messageBuilder = vitalSignMessageBuilder;
        }

        public void NewDataRequestReceived(IPEndPoint target)
        {
            lastRequestTimestamp.AddOrUpdate(target.Address, DateTime.UtcNow, (key, obj) => DateTime.UtcNow);
            StoppableTask streamingTask;
            if(!streamingTasks.TryGetValue(target.Address, out streamingTask)
                || streamingTask.Task.Status.InSet(TaskStatus.RanToCompletion, TaskStatus.Canceled, TaskStatus.Faulted))
            {
                streamingTask?.Stop();
                var targetIpAddress = target.Address;
                var cancellationTokenSource = new CancellationTokenSource();
                streamingTask = new StoppableTask(StartStreaming(target, cancellationTokenSource.Token), cancellationTokenSource);
                streamingTasks.AddOrUpdate(targetIpAddress, streamingTask, (key, obj) => streamingTask);
            }
        }

        async Task StartStreaming(IPEndPoint target, CancellationToken cancellationToken)
        {
            var sourcePort = Informations.VitalSignSourcePort;
            using (var udpClient = new UdpClient(sourcePort))
            {
                while (!cancellationToken.IsCancellationRequested
                       && DateTime.UtcNow - lastRequestTimestamp[target.Address] < streamingTimeout)
                {
                    var messageBytes = messageBuilder.Build();
                    await udpClient.SendAsync(messageBytes, messageBytes.Length, target);
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }
            streamingTasks.TryRemove(target.Address, out _);
        }

        public void StopStreaming()
        {
            foreach (var streamingTask in streamingTasks.Values)
            {
                streamingTask.Stop();
            }
        }

        public void Dispose()
        {
            StopStreaming();
        }
    }
}