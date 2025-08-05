using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using IncidentEventService.EventHandlers;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Event;

namespace IncidentEventService;

public class CadEventService
{
    private readonly CADManager _cadManager;
    private readonly Channel<object> _eventChannel;
    private readonly Task _processingTask;
    
    private readonly IncidentAddressChangedHandler _incidentAddressChangedHandler;
    private readonly IncidentClosedHandler _incidentClosedHandler;
    private readonly IncidentProblemChangedHandler _incidentProblemChangedHandler;
    private readonly PendingIncidentCreatedHandler _pendingIncidentCreatedHandler;


    public CadEventService(
        CADManager cadManager,
        IncidentAddressChangedHandler incidentAddressChangedHandler,
        IncidentClosedHandler incidentClosedHandler,
        IncidentProblemChangedHandler incidentProblemChangedHandler,
        PendingIncidentCreatedHandler pendingIncidentCreatedHandler)
    {
        _cadManager = cadManager;
        _eventChannel = Channel.CreateBounded<object>(1000);
        _processingTask = ProcessQueueAsync();
        
        _incidentAddressChangedHandler = incidentAddressChangedHandler;
        _incidentClosedHandler = incidentClosedHandler;
        _incidentProblemChangedHandler = incidentProblemChangedHandler;
        _pendingIncidentCreatedHandler = pendingIncidentCreatedHandler;
    }
    
    /// <summary>
    /// Handles the incoming CAD events by dispatching them to the appropriate handler.
    /// </summary>
    /// <param name="genericEvent"></param>
    private async Task HandleEventAsync(object genericEvent)
    {
        switch (genericEvent)
        {
            case IncidentProblemChanged incidentProblemChanged:
                await _incidentProblemChangedHandler.HandleIncidentProblemChanged(_cadManager, incidentProblemChanged);
                break;
            case PendingIncidentCreated pendingIncidentCreated:
                await _pendingIncidentCreatedHandler.HandlePendingIncidentCreated(_cadManager, pendingIncidentCreated);
                break;
            case IncidentAddressChanged incidentAddressChanged:
                await _incidentAddressChangedHandler.HandleIncidentAddressChanged(_cadManager, incidentAddressChanged);
                break;
            case IncidentClosed incidentClosed:
                await _incidentClosedHandler.HandleIncidentClosed(_cadManager, incidentClosed);
                break;
        }
    }
    
    // ------------------------- DO NOT MODIFY BELOW THIS LINE ------------------------- //

    private async Task<bool> TryWriteWithRetryAsync(object genericEvent, int maxRetries = 3)
    {
        for (var i = 0; i < maxRetries; i++)
        {
            if (await _eventChannel.Writer.WaitToWriteAsync() && _eventChannel.Writer.TryWrite(genericEvent))
                return true;
            await Task.Delay(100); // wait before retrying
        }
        return false;
    }

    public async void CadEventReceivedAsync(object sender, CADEventReceivedArgs e)
    {
        try
        {
            foreach (var genericEvent in e.CADEvent.BusinessEvents)
            {
                var success = await TryWriteWithRetryAsync(genericEvent);
                if (!success)
                {
                    await Console.Error.WriteLineAsync($"Failed to enqueue CAD event {genericEvent.GetType()} after retries.");
                }
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Exception in CadEventReceivedAsync: {ex}");
        }
    }

    private async Task ProcessQueueAsync()
    {
        await foreach (var genericEvent in _eventChannel.Reader.ReadAllAsync())
        {
            try
            {
                await HandleEventAsync(genericEvent);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Error processing event: {ex}");
            }
        }
    }
    
    public async Task StopAsync()
    {
        _eventChannel.Writer.Complete();
        await _processingTask;
    }
}