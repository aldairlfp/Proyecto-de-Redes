using System.Text;

namespace PhysicalLayer
{
    public class Device
    {
        // Representar los puertos que 
        // que tiene el dispositivo.
        protected Port[] Ports { get; private set; }

        // Nombre que tiene el dispositivo
        protected string Name { get; private set; }

        // Representa el bit de salida del dispositivo 
        // inicialmente este es None
        protected Bit OutBit { get; set; }

        // Representa el bit de entrada del dispositivo
        // inicialmente este es None
        protected Bit InputBit { get; private set; }

        // Este es para saber en el montículo del programa inicial donde 
        // se almacenan los dispositivos, que indice tiene el en ese array 
        protected int Index { get; private set; }

        // Esto es para representar la cantidad de puertos que tiene el dispositivo
        protected int NumberOfPorts { get; private set; }

        // Constructor de un dispositivo en general 
        // Como es obvio todo dispositivo tiene un nombre , un indice 
        // y la  cantidad de puertos que este va a tener
        public Device(string name, int numberOfPorts, int index)
        {
            Name = name;
            OutBit = Bit.none;
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
        public IEnumerable<Port> PuertosConectados
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
            string completeRoute = Path.Join(OutputDirectory(), Name + ".txt");

            //se crea el archivo si no existe y lo abre si ya existe 
            using (StreamWriter mylogs = File.AppendText(completeRoute))
            {
                mylogs.WriteLine(receive);
                mylogs.Close();
            }
        }

        // Retorna el path del directorio de salida donde se va a escribir 
        // donde se van a crear los ficheros para escribir la salidas correspondientes
        string OutputDirectory()
        {
            var currentDirectory = Environment.CurrentDirectory;
            var parent = Directory.GetParent(Directory.GetParent(Directory.GetParent(currentDirectory).FullName).FullName);
            return Path.Join(parent.FullName, "output");
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
        public bool TWColision()
        {
            InputBit = Bit.none;
            bool cero = false, uno = false;

            foreach (var item in this.PuertosConectados)
            {
                if (item.Inputs[(int)Bit.cero]) cero = true;
                if (item.Inputs[(int)Bit.uno]) uno = true;
            }


            if (uno && cero) return true;
            else if (uno) InputBit = Bit.uno;
            else if (cero) InputBit = Bit.cero;

            if (this is Computadora && InputBit != Bit.none && InputBit != OutBit) return true;
            else return false;
        }

        // Chequea si hubo colisión y escribe en las salidas los 
        // valores correspondientes 
        public virtual void ProcessOutputInput()
        {
            bool colision = TWColision();

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

            StringBuilder salida = new StringBuilder();

            for (int i = 0; i < this.NumberOfPorts; i++)
            {
                if (Ports[i] == null || !Ports[i].IsConnected) continue;

                if (Ports[i].Inputs[(int)Bit.cero] || Ports[i].Inputs[(int)Bit.uno])
                {
                    salida.Append(string.Format("{0} {1} receive {2} \n", Program.current_time, this.Name + $"_{i + 1}", (int)this.bitentrada));
                }
                else
                {
                    salida.Append(string.Format("{0} {1} send {2} \n", Program.current_time, this.Name + $"_{i + 1}", (int)this.bitentrada));
                }
            }

            while (salida.Length > 1 && salida[salida.Length - 1] == '\n')
                salida.Remove(salida.Length - 1, 1);

            EscribirEnLaSalida(salida.ToString());


            LimpiarLosParametrosDeEntrada();
        }


        /// <summary>
        /// esto limpia el array de bit recibido por las computadoras en un 
        /// mili segundo determinado 
        /// </summary>
        protected void CleanInputParameters()
        {

            foreach (var item in this.PuertosConectados)
            {
                item.LimpiarEntradas();
            }

            this.BitdeEntrada = Bit.none;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append($"{this.name} \n");

            stringBuilder.Append($"Bit de Salida:\t {(int)this.BitdeSalida}\n");

            stringBuilder.Append($"Bit de Entradas:\n");

            foreach (var item in this.PuertosConectados)
            {
                for (int j = 0; j < item.Entradas.Length; j++)
                {
                    stringBuilder.Append($"{ (item.Entradas[j] == true ? "T" : "F")}  ");
                }
                stringBuilder.Append(Environment.NewLine);
            }

            stringBuilder.Append(Environment.NewLine);

            return stringBuilder.ToString();
        }

    }
}