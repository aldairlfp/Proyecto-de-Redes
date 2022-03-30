public class Computer:Device
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

        public Computer(string name ,int index) : base(name ,1, index)
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
            WaitingTimeSending = (uint)new Random().Next(5,50);
            Console.WriteLine($"{name} going to wait {WaitingTimeSending} to send another data");
            SendingTime = 0;
        }
        
       
        /// <summary>
        /// Este método se llama cuando hubo una instrucción 
        /// send para el envío de un paquete de bits
        /// </summary>
        /// <param name="paquete"></param>
        public void send(Bit [] package)
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
            if (ports[0] == null || !ports[0].IsConnectOtherDevice)
                return false; 
            UpdateOutBit();
            SendOutBitInOtherComputer();
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
            if (ports[0] == null || !ports[0].IsConnectToOtherDevice)  return;

            if (SendQueve.Count == 0)
            {
                OutBit = Bit.none;
            }
            else
            {

                if (WaitingTimeSending > 0)
                {
                    WaitingTimeSending--;
                    OutBit = Bit.none;
                    return;
                }

                OutBit = SendQueve.Peek();

                SendingTime++;

                if (SendingTime >= Program.signal_time)
                {
                    SendingTime = 0;
                    FirstTimeSending = Program.current_time;
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
        public void SendOutBitInOtherComputer()
        {
            Queue<Device> queue = new Queue<Device>();
            bool[] mask = new bool[Program.Device.Count];
            mask[index]= true; 
            queue.Enqueue(this);

            Device current; 

            while(queue.Count>0)
            {
                current = queue.Dequeue();

                foreach (var item in current.ConnectPorts)
                {
                    Device disconnect = item.ConnectDevice;
                    if (mask[disconnect.Index]) continue;

                    int PortByWhichItIsConnected = item.NumberPortConnected;
                   
                    disconnect.ReceiveBit(PortByWhichItIsConnected, OutBit); 
                    
                    mask[disconnect.Index] = true;
                    queue.Enqueue(disconnect); 
                }
            }
        }

        /// <summary>
        /// Este método es llamado para una vez que se establecieron las
        /// salidas y entradas de datos a esta computadora puedan ser 
        /// procesados estos datos y determinar si hubo una colisión 
        /// y escribir en la salida del dispositivo los datos 
        /// de salida  correspondiente a esta computadora.
        /// </summary>
        public override void ProcessInformationInAndOut()
        {
            if (ports[0] == null || !ports[0].IsConnectToOtherDevice)
                return;

            bool thereWasACollision = ThereWasACollision();

            
            if (OutBit != Bit.none && thereWasACollision)
            {
                WriteOut(string.Format("{0} {1} send {2} collision", Program.current_time, this.Name, (int)this.OutBit));
                Update();
                base.CleanEntranceParameters(); 
                return; 
            }
            else if (this.OutBit != Bit.none)
            {
                WriteOut(string.Format("{0} {1} send {2} Ok", Program.current_time, Name, (int)OutBit));
            }
            
            if (this.InBit != Bit.none)
            {
                WriteOut(string.Format("{0} {1} receive {2} Ok", Program.current_time, Name, (int)InBit));
            }

            base.CleanEntranceParameters();
        }
    }