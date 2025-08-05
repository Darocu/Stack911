using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using TriTech.VisiCAD;
using TriTech.VisiCAD.Event;
using UnitEventService.EventHandlers;

namespace UnitEventService;

public class CadEventService
{
    private readonly CADManager _cadManager;
    private readonly Channel<object> _eventChannel;
    private readonly Task _processingTask;

    private readonly UnitStatusChangedToAvailableHandler _unitStatusChangedToAvailableHandler;
    // private readonly UnitStatusChangedToTransportHandler _unitStatusChangedToTransportHandler;
    // private readonly UnitStackedIncidentsChangedHandler _unitStackedIncidentsChangedHandler

    public CadEventService(
        CADManager cadManager,
        UnitStatusChangedToAvailableHandler unitStatusChangedToAvailableHandler
        // UnitStatusChangedToTransportHandler unitStatusChangedToTransportHandler,
        // UnitStackedIncidentsChangedHandler unitStackedIncidentsChangedHandler
        )
    {
        _cadManager = cadManager;
        _eventChannel = Channel.CreateBounded<object>(1000);
        _processingTask = ProcessQueueAsync();
        
        _unitStatusChangedToAvailableHandler = unitStatusChangedToAvailableHandler;
        // _unitStatusChangedToTransportHandler = unitStatusChangedToTransportHandler;
        // _unitStackedIncidentsChangedHandler = unitStackedIncidentsChangedHandler;
    }
    
    /// <summary>
    /// Handles the incoming CAD events by dispatching them to the appropriate handler.
    /// </summary>
    /// <param name="genericEvent"></param>
    private async Task HandleEventAsync(object genericEvent)
    {
        switch (genericEvent)
        {
            case UnitStatusChangedToAvailable unitStatusChangedToAvailable:
                await _unitStatusChangedToAvailableHandler.HandleUnitStatusChangedToAvailable(_cadManager, unitStatusChangedToAvailable);
                break;
            // case UnitStatusChangedToTransport:
            //    await _unitStatusChangedToTransportHandler.HandleUnitStatusChangedToTransport(_cadManager, unitStatusChangedToTransport);
            //    break;
            // case UnitStackedIncidentsChanged:
            //    await _unitStackedIncidentsChangedHandler.HandleUnitStackedIncidentsChanged(_cadManager, unitStackedIncidentsChanged);
            //    break;
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