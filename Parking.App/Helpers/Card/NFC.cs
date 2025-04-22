using MiFare.Classic;
using MiFare.Devices;
using MiFare.PcSc;
using MiFare;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Helpers.Card;

public class NFC
{
    private SmartCardReader reader;
    private MiFareCard card;
    private IReadOnlyList<string> readers;

    private bool verbose = false;

    public delegate void CardSectorReceivedEventHandler(int selectedSector, byte[] data);
    public event CardSectorReceivedEventHandler CardSectorReceived;

    public delegate void CardUidReceivedEventHandler(byte[] uid);
    public event CardUidReceivedEventHandler CardUidReceived;

    public int selectedSector = 0;

    public NFC()
    {
        readers = GetReaders();
    }

    public void Init(int deviceId, bool _verbose = false)
    {
        this.verbose = _verbose;
        GetDevices(deviceId);
    }

    public void SetSelectedSector(int _selectedSector)
    {
        this.selectedSector = _selectedSector;
    }

    private IReadOnlyList<string> GetReaders()
    {
        return CardReader.GetReaderNames();
    }

    private async void GetDevices(int deviceId)
    {
        try
        {
            reader = await CardReader.FindAsync(readers[deviceId]);
            if (reader == null)
            {
                Log("No Readers Found");
                return;
            }

            reader.CardAdded += CardAdded;
            reader.CardRemoved += CardRemoved;
        }
        catch (Exception e)
        {
            Log("Exception: " + e.Message);
        }
    }


    private void CardRemoved(object sender, EventArgs e)
    {
        card?.Dispose();
        card = null;
    }

    private async void CardAdded(object sender, CardEventArgs args)
    {
        try
        {
            await HandleCard(args);
        }
        catch (Exception ex)
        {
            Log("CardAdded Exception: " + ex.Message);
        }
    }

    private async Task HandleCard(CardEventArgs args)
    {
        try
        {
            card?.Dispose();
            card = args.SmartCard.CreateMiFareCard();
            //var localCard = card;
            //var cardIdentification = await localCard.GetCardInfo();
            CardUidReceived?.Invoke(await card.GetUid());
        }
        catch (Exception e)
        {
            Log("HandleCard Exception: " + e.Message);
        }
    }

    private void Log(string message)
    {
        if (verbose)
        {
            Debug.WriteLine(message);
        }
    }
}
