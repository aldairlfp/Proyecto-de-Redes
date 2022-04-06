namespace PhysicalLayer
{
    public class Computer : Device
    {
        /// <summary>
        /// Esto es una cola para almacenar los bits que quedan 
        /// por enviar aun. Si la cola esta vacía es que no quedan 
        /// bit por enviar. 
        /// </summary>
        Queue<Bit> SendQueve;


        /// <summary>
        /// Este es el tiempo que el bit que esta enviándose 
        /// ha estado transmitiéndose a las demás computadoras. 
        /// </summary>
        uint SendingTime;


        uint FirstTimeSending;


        /// <summary>
        /// esto es para determinar el tiempo que ha estado la
        /// computadora sin enviar información producto de una 
        /// colisión que detecto anteriormente
        /// </summary>
        uint WaitingTimeSending;

        public Computer(string name, int index) : base(name, 1, index)
        {
            SendingTime = 0;
            FirstTimeSending = 0;
            SendQueve = new Queue<Bit>();
        }


        /// <summary>
        /// Este método se llama cuando se detecta una colisión 
        /// Esto es para darle un tiempo random a la computadora 
        /// para que espere anted de volver a enviar un bit
        /// </summary>
        public void Update()
        {
            WaitingTimeSending = (uint)new Random().Next(5, 50);
            Console.WriteLine($"{Name} going to wait {WaitingTimeSending} to send another data");
            SendingTime = 0;
        }


        /// <summary>
        /// Este método se llama cuando hubo una instrucción 
        /// send para el envío de un paquete de bits
        /// </summary>
        /// <param name="paquete"></param>
        public void send(Bit[] package)
        {
            foreach (var item in package)
            {
                SendQueve.Enqueue(item);
            }
        }

        /// <summary>
        /// Este método se llama inicialmente antes de cualquier otro 
        /// llamado en un mili segundo , y lo que hace básicamente es determinar 
        /// que bit se va a enviar y Enviárselo a las demás computadoras que 
        /// están conectadas en la red que se encuentra la de esta instancia
        /// </summary>
        /// <returns></returns>
        public bool SendInformationOtherComputer()
        {
            if (Ports[0] == null || !Ports[0].IsConnected)
                return false;
            UpdateOutBit();
            SendOutputBitToOtherComputer();
            return true;
        }


        /// <summary>
        /// Este método pone el bit de salida que le corresponde
        /// es ese mili segundo estar en la salida . Este método se 
        /// llama una sola vez en cada mili segundo de ejecución del 
        /// programa
        /// </summary>
        public void UpdateOutBit()
        {
            if (Ports[0] == null || !Ports[0].IsConnected) return;

            if (SendQueve.Count == 0)
            {
                OutputBit = Bit.none;
            }
            else
            {

                if (WaitingTimeSending > 0)
                {
                    WaitingTimeSending--;
                    OutputBit = Bit.none;
                    return;
                }

                OutputBit = SendQueve.Peek();

                SendingTime++;

                if (SendingTime >= Config.SignalTime)
                {
                    SendingTime = 0;
                    FirstTimeSending = Config.CurrentTime;
                    SendQueve.Dequeue();
                }
            }
        }

        /// <summary>
        /// Este método se llama después de haber llamado al método
        ///  ActualizarElBitDeSalida() para que este pueda se enviado 
        ///  con el procedimiento de este método a las demás computadoras 
        ///  que están conectadas a la que representa esta instancia, 
        ///  (aquí lo que se usa es un bfs para enviar el bit a cada computadora)
        /// </summary>
        public void SendOutputBitToOtherComputer()
        {
            Queue<Device> queue = new Queue<Device>();
            bool[] mask = new bool[Simulation.Devices.Count];
            mask[Index] = true;
            queue.Enqueue(this);

            Device current;

            while (queue.Count > 0)
            {
                current = queue.Dequeue();

                foreach (var item in current.ConnectedPorts)
                {
                    Device deviceConnected = item.Host;
                    if (mask[deviceConnected.Index]) continue;

                    int PortByWhichItIsConnected = item.PortNumberConnectedTo;

                    deviceConnected.RecieveABit(PortByWhichItIsConnected, OutputBit);

                    mask[deviceConnected.Index] = true;
                    queue.Enqueue(deviceConnected);
                }
            }
        }

        // Este método es llamado para una vez que se establecieron las
        // salidas y entradas de datos a esta computadora puedan ser 
        // procesados estos datos y determinar si hubo una colisión 
        // y escribir en la salida del dispositivo los datos 
        // de salida  correspondiente a esta computadora.
        public override void ProcessOutputInput()
        {
            if (Ports[0] == null || !Ports[0].IsConnected)
                return;

            bool thereWasACollision = TWCollision();

            if (OutputBit != Bit.none && thereWasACollision)
            {
                WriteOutput(string.Format("{0} {1} send {2} collision", Simulation.CurrentTime, this.Name, (int)this.OutputBit));
                Update();
                base.CleanInputParameters();
                return;
            }
            else if (this.OutputBit != Bit.none)
            {
                WriteOutput(string.Format("{0} {1} send {2} Ok", Simulation.CurrentTime, Name, (int)OutputBit));
            }

            if (this.InputBit != Bit.none)
            {
                WriteOutput(string.Format("{0} {1} receive {2} Ok", Simulation.CurrentTime, Name, (int)InputBit));
            }

            base.CleanInputParameters();
        }
    }
}