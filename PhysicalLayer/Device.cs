using System.Text;

namespace PhysicalLayer
{
    public class Device
    {
        // Representar los puertos que 
        // que tiene el dispositivo.
        public Port[] Ports { get; private set; }

        // Nombre que tiene el dispositivo
        public string Name { get; private set; }

        // Representa el bit de salida del dispositivo 
        // inicialmente este es None
        public Bit OutputBit { get; set; }

        // Representa el bit de entrada del dispositivo
        // inicialmente este es None
        public Bit InputBit { get; private set; }

        // Este es para saber en el montículo del programa inicial donde 
        // se almacenan los dispositivos, que indice tiene el en ese array 
        public int Index { get; private set; }

        // Esto es para representar la cantidad de puertos que tiene el dispositivo
        public int NumberOfPorts { get; private set; }

        // Constructor de un dispositivo en general 
        // Como es obvio todo dispositivo tiene un nombre , un indice 
        // y la  cantidad de puertos que este va a tener
        public Device(string name, int numberOfPorts, int index)
        {
            Name = name;
            OutputBit = Bit.none;
            InputBit = Bit.none;
            Index = index;
            NumberOfPorts = numberOfPorts;

            Ports = new Port[NumberOfPorts];

            for (int i = 0; i < NumberOfPorts; i++)
            {
                Ports[i] = new Port(Name + $"_{i}", i);
            }
        }

        // Retorna la instancia del Puerto que corresponde al indice 
        // que se pasa por el id
        public Port GetPort(int id)
        {
            if (id >= this.NumberOfPorts || id < 0)
                throw new IndexOutOfRangeException("El dispositivo no tiene el puerto que se le especifica");

            return Ports[id];
        }

        // Esto retorna todos los puertos (en orden ) que 
        // tienen conectado algún dispositivo
        public IEnumerable<Port> ConnectedPorts
        {
            get
            {
                foreach (var item in Ports)
                    if (item.IsConnected)
                        yield return item;
            }
        }

        // Crea un fichero si no existe , y si existe lo abre y 
        // escribe lo que se le pasa por el parámetro indicado en recibo
        // El txt que se crea o abre corresponde al dispositivo que es instanciado 
        // por esta clase , (nombredeldispositivo .txt)
        public void WriteOutput(string receive)
        {
            string completeRoute = Path.Join(OutputDirectory, Name + ".txt");
            System.Console.WriteLine(completeRoute);
            //se crea el archivo si no existe y lo abre si ya existe 
            using (StreamWriter mylogs = File.AppendText(completeRoute))
            {
                mylogs.WriteLine(receive);
                mylogs.Close();
            }
        }

        // Retorna el path del directorio de salida donde se va a escribir 
        // donde se van a crear los ficheros para escribir la salidas correspondientes
        string OutputDirectory
        {
            get
            {
                var currentDirectory = Environment.CurrentDirectory;
                return Path.Join(currentDirectory, "output");
            }
        }

        // Escribe el bit que se pasa por el parámetro Bit 
        // en el puerto que se indica en el parámetro puerto
        public void RecieveABit(int port, Bit bit)
        {
            Ports[port].RecieveABit(bit);
        }

        // Detecta si en los bits de entradas hubo mas de 
        // un bit que fue recibido , es decir se recibieron 
        // bit diferentes de algunas computadoras
        public bool TWCollision()
        {
            InputBit = Bit.none;
            bool cero = false, uno = false;

            foreach (var item in this.ConnectedPorts)
            {
                if (item.Inputs[(int)Bit.cero]) cero = true;
                if (item.Inputs[(int)Bit.uno]) uno = true;
            }


            if (uno && cero) return true;
            else if (uno) InputBit = Bit.uno;
            else if (cero) InputBit = Bit.cero;

            if (this is Computer && InputBit != Bit.none && InputBit != OutputBit) return true;
            else return false;
        }

        // Chequea si hubo colisión y escribe en las salidas los 
        // valores correspondientes 
        public virtual void ProcessOutputInput()
        {
            System.Console.WriteLine("llegue");
            bool colision = TWCollision();

            if (colision)
            {
                CleanInputParameters();
                return;
            }

            if (this.InputBit == Bit.none)
            {
                CleanInputParameters();
                return;
            }

            StringBuilder output = new StringBuilder();

            for (int i = 0; i < this.NumberOfPorts; i++)
            {
                if (Ports[i] == null || !Ports[i].IsConnected) continue;

                if (Ports[i].Inputs[(int)Bit.cero] || Ports[i].Inputs[(int)Bit.uno])
                {
                    output.Append(string.Format("{0} {1} receive {2} \n", Simulation.CurrentTime, this.Name + $"_{i + 1}", (int)this.InputBit));
                }
                else
                {
                    output.Append(string.Format("{0} {1} send {2} \n", Simulation.CurrentTime, this.Name + $"_{i + 1}", (int)this.InputBit));
                }
            }

            while (output.Length > 1 && output[output.Length - 1] == '\n')
                output.Remove(output.Length - 1, 1);

            System.Console.WriteLine(output);
            WriteOutput(output.ToString());

            CleanInputParameters();
        }


        /// <summary>
        /// esto limpia el array de bit recibido por las computadoras en un 
        /// mili segundo determinado 
        /// </summary>
        protected void CleanInputParameters()
        {

            foreach (var item in this.ConnectedPorts)
            {
                item.CleanInputs();
            }

            this.InputBit = Bit.none;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append($"{this.Name} \n");

            stringBuilder.Append($"Bit de Salida:\t {(int)this.OutputBit}\n");

            stringBuilder.Append($"Bit de Entradas:\n");

            foreach (var item in this.ConnectedPorts)
            {
                for (int j = 0; j < item.Inputs.Length; j++)
                {
                    stringBuilder.Append($"{ (item.Inputs[j] == true ? "T" : "F")}  ");
                }
                stringBuilder.Append(Environment.NewLine);
            }

            stringBuilder.Append(Environment.NewLine);

            return stringBuilder.ToString();
        }

    }
}