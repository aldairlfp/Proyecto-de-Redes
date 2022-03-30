namespace PhysicalLayer;

public class Port
{
    // Representa el id que tiene el puerto en
    // el dispositivo al que pertenece
    public string IdPort { get; private set; }

    // Representa el nombre del puerto
    // con el id correspondiente
    public int PortNumber { get; private set; }

    // Esta es para tener la instancia del dispositivo que 
    // esta conectado por este puerto
    public Device Host { get; set; }

    // Nombre del puerto al que esta conectado 
    public string ConnectedPort { get; set; }

    // Bit de salida que hay en este dispositivo
    public Bit OutBit { get; set; }

    // Bits se han recibido de algun 
    // dispositivo en especifico en un mili segundo determinado
    public bool[] Inputs { get; private set; }

    /// Si el dispositivo esta conectado con algún 
    /// otro dispositivo 
    public bool IsConnected { get; set; }

    // Constructor del puerto con su nombre y su id
    public Port(string idPort, int portNumber)
    {
        IdPort = idPort;
        OutBit = Bit.none;
        PortNumber = portNumber;

        Inputs = new bool[Enum.GetNames(typeof(Bit)).Length];
    }

    // Retorna el indice del puerto al que esta conectado 
    // el dispositivo por el puerto actual 
    public int PortNumberConnectedTo => (int.Parse(this.ConnectedPort.Split('_')[1]) - 1);

    // Esto te desconecta el puerto actual de cualquier otro puerto 
    // al que se encuentre conectado 
    public void DisconnectPort()
    {
        ConnectedPort = null;
        Host = null;
        OutBit = Bit.none;
        IsConnected = false;

        CleanInputs();
    }

    // Pone todas las entradas como si no hubiera recibido 
    // ningún bit de información 
    public void CleanInputs()
    {
        for (int i = 0; i < Inputs.Length; i++)
            Inputs[i] = false;
    }

    // Actualiza y pone en las entradas el bit que se recibio
    public void RecieveABit(Bit bit)
    {
        Inputs[(int)bit] = true;
    }
}