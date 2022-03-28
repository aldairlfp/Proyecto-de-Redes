using System.Text;
namespace PhysicalLayer;

public enum Bit
{
    Zero = 0,
    One = 1,
    None = 2
}

public enum Action
{
    Received = 0,
    Send = 1
}

public enum ActionResult
{
    Ok = 0,
    Received = 1,
    Collision = 2,
    None = 3,
}

// para hacer algunos metdos auxiliares
public static class CheckMetods
{
    public static bool CheckIsOkDirMac(string dirMac)
    {
        throw new NotImplementedException();
    }
    public static bool IsBinaryString(string s)
    {
        throw new NotImplementedException();
    }
    public static bool IsValidPortHub(int a, int b)
    {
        throw new NotImplementedException();
    }
}

public class Port : IPuerto
{
    string portId;
    int portNumber;
    Bit outBit;
    Bit inBit;
    Queue<Bit> queueOutput;
    Dispositivo DeviceBelong;
    string dirMac;
    protected ICable cable;
    private int timeSending = 0;
    private List<OneBitPackage> history;
    private int timeReceived = 0;
    private int timeReceivedByte = 0;
    private Bit AuxInBit = Bit.none;
    public int PortNumber
    {
        get => this.portNumber;
    }
    public Port(string idPort, int numberPort, Device device)
    {
        this.portId = idPort;
        this.outBit = Bit.none;
        this.portNumber = numberPort;
        this.cable = null;
        this.DeviceBelong = device;
        this.queueOutput = new Queue<Bit>();
        this.history = new List<OneBitPackage>();
    }
    public void SendData(List<Bit> datatosend)
    {
        foreach (var item in datatosend)
        {
            this.queueOutput.Enqueue(item);
        }
    }
    public Device DeviceBelong
    {
        get => this.deviceBelongs;
    }
    public List<OneBitPackage> GiveMeHistory
    {
        get => this.history;
    }
    public bool ConnectCableToPort(Cable cable)
    {
        if (cable == null)
        {
            return false;
        }
        this.cable = cable;
        return true;
    }
    public string IDPuerto
    {
        get => this.portId;
    }
    public Bit OutBit
    {
        get => this.outBit;
        set => this.outBit = value;
    }
    public Bit InBit
    {
        get => this.inBit;
        set => this.inBit = value;
    }
    public Bit GiveMeOutBit()
    {
        return this.OutBit;
    }
    public Bit GiveMeInBit()
    {
        if (this.cable == null) return Bit.none;

        Port otherPort = this.cable.puerto1.Equals(this) ?
            this.cable.puerto2 :
            this.cable.puerto1;
        return otherPort.GiveMeOutBit();
    }
    public Cable Cable
    {
        set => this.cable = value;
    }
}
