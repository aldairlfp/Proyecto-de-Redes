namespace PhysicalLayer;

public class Port
{
    // Representa el id que tiene el puerto en
    // el dispositivo al que pertenece
    public string IdPort { get; private set; }
    // Representa el nombre del puerto
    // con el id correspondiente
    public int PortNumber { get; private set; }
    public Device Host { get; private set; }
    public string ConnectedPort { get; private set; }
    public Bit OutBit { get; private set; }

    public Port(string name, Device host)
    {
        
    }
}