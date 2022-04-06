using System.Collections.Generic;

namespace PhysicalLayer
{
    public static class Simulation
    {
        // Mili-segundo actual , por el que va ejecutándose el programa
        public static uint CurrentTime = 0;

        // Tiempo que demora un bit transmitiéndose por un canal 
        public static uint SignalTime = 10;

        // Tiempo máximo que puede corres el programa (la medida de tiempo es como mili-segundos)
        public static uint MaxTime = 1000;

        // Cantidad mínima de puertos que puede tener un hub
        public static int MinPortInHub = 4;

        // Cantidad máxima de puertos que puede tener un hub
        public static int MaxPortInHub = 8;

        // Cola de instrucciones que son cargadas al principio del programa 
        // para después ser ejecutadas según llegue su momento 
        public static Queue<Instruction>? Instructions;

        // Lista de dispositivos que actualmente en el entorno 
        // El tamaño de la sita va creciendo a medida que se ejecute 
        // correctamente una instrucción de create 
        public static List<Device>? Devices;

        public static void RunAplication()
        {
            Devices = new List<Device>();

            //Esto limpia el directorio de la output (es decir borra todos los ficheros que hay 
            //en el directorio '/output') para que en la ejecución no se vayan a sobre escribir 
            //sobre ficheros ya existentes 
            CleanOutputDirectory();

            //Esta es para cargar todos las instrucciones que hay en el fichero 'script.txt' 
            //para almacenarlos en memoria , todas las instrucciones que hay en el fichero quedan 
            //almacenadas en instrucciones , ordenadas por el tiempo de ejecución de la instrucción 
            //de forma ascendente, para que una vez hallan sido ejecutadas salgan de la cola.  
            LoadInstructions();

            //Este métodos es para configurar todo el entorno del programa ,como signal_time , cantidad
            //máxima de mili-segundos que debe correr el programa , etc 
            Config();

            //Este es el ciclo principal para correr las instrucciones y hacer el envió de 
            //información entre todos los host que están conectados.
            while (CurrentTime < MaxTime)
            {

                // Console.WriteLine($"CURRENT TIME : {CurrentTime} mili-second");
 
                //Ejecutar las instrucciones que corresponden a ejecutarse en el 
                //mili-segundo actual que están en la cola de instrucciones ; 
                foreach (var item in NextInstructionsToExecute(CurrentTime))
                {
                    ExecuteInstruction(item);
                }

                //Actualizar el bit de output de cada computadora para después 
                //enviar el bit que esta en la output a cada uno de los 
                //Dispositivos a los que esta conectado la Computadora
                foreach (var item in Devices.Where(e => e is Computer))
                {
                    Computer? comp = item as Computer;
                    comp.SendOutputBitToOtherComputer();
                }

                //verifica las entrada por cada dispositivo y chequea su hubo 
                //una colisión, y escribe en la output (en su txt correspondiente )
                //la output que este dispositivo tiene. 
                foreach (var item in Devices)
                {
                    item.ProcessOutputInput();
                }

                //Aumentar el contador de mili-segundos para pasar a procesar 
                //el próximo mili-segundo para ejecutar las instrucciones
                CurrentTime++;
            }
        }

        // Esto es para poner la configuracion por defecto , 
        // que se pone de la manera que esta descrita en el 
        // metodo
        public static void SetDefaultConfig()
        {
            MinPortInHub = 4;
            MaxPortInHub = 8;
            SignalTime = 10;
            MaxTime = 100;
        }

        // Lee del fichero config.txt que se 
        // encuentra en este mismo directorio en que esta este proyecto 
        // este pone todos los parametros establecido que se encuentran en
        // el fichero 
        public static void Config()
        {
            var CurrentDirectory = Environment.CurrentDirectory;
            string fullDirectory = Path.Join(CurrentDirectory, "config.txt");

            if (!File.Exists(fullDirectory))
            {
                SetDefaultConfig();
                Console.WriteLine($"Advertencia: El fichero 'config.txt' no existe\n las configuraciones que se van a poner son las que hay por defecto");
                return;
            }

            var variable = File.ReadLines(fullDirectory);

            foreach (var item in variable)
            {
                if (item.Length < 1) continue;

                string[] startConfig = item.Split(':');

                if (startConfig.Length < 2) throw new InvalidCastException($"la configuración {item} no tiene el formato correcto");

                switch (startConfig[0])
                {
                    case "max_cantidad_milisegundos":
                        UInt32 maxTime;
                        if (UInt32.TryParse(startConfig[1], out maxTime))
                        {
                            MaxTime = maxTime;
                        }
                        else
                        {
                            throw new InvalidCastException($"el numero para asignarle al tiempo_maximo: '{startConfig[1]} no es valido '");
                        }
                        break;

                    case "signal_time":
                        int signalTime;
                        if (Int32.TryParse(startConfig[1], out signalTime))
                        {
                            SignalTime = (uint)signalTime;
                        }
                        else
                        {
                            throw new InvalidCastException($"el numero para asignarle el _signal_time '{startConfig[1]}' no tiene el formato correcto");
                        }
                        break;
                    case "numero_puertos_hub":
                        string[] intervalEdges = startConfig[1].Split('-');

                        if (intervalEdges.Length < 2)
                        {
                            throw new InvalidCastException($"No tiene el formato correcto los intervalos '{intervalEdges}'");
                        }

                        int min, max;

                        if (!int.TryParse(intervalEdges[0], out min))
                        {
                            throw new InvalidCastException($"El extremo {intervalEdges[0]} no es un numero valido ");
                        }
                        if (!int.TryParse(intervalEdges[1], out max))
                        {
                            throw new InvalidCastException($"El extremo {intervalEdges[1]} no es un numero valido ");
                        }

                        if (IsValidPortNumber(min, max))
                        {
                            MinPortInHub = min;
                            MaxPortInHub = max;
                        }
                        break;

                }
            }
        }

        public static bool IsValidPortNumber(int a, int b)
        {
            if (a < 4)
            {
                throw new InvalidCastException("Un hub no puede tener menos de 4 puertos ");
            }
            if (b <= a)
            {
                throw new InvalidCastException("La cantidad máxima de puertos no puede ser menor o igual que la cantidad mínima de puertos");
            }
            return true;
        }

        // esto borra todos los ficheros que se encuentran en el directorio 
        // output y se llama al principio de la ejecución del programa , antes de 
        // entrar en el ciclo principal , y se hace para barrar todos los fichero que 
        // se podían haber generado previamente en la ejecución de programa en un momento 
        // anterior
        public static void CleanOutputDirectory()
        {
            var CurrentDirectory = Environment.CurrentDirectory;

            var output = Path.Join(CurrentDirectory, "output");

            if (Directory.Exists(output))
            {
                foreach (var item in Directory.GetFiles(output))
                {
                    File.Delete(item);
                }
            }
            else
            {
                throw new Exception("La Salida no existe , para borrar el contenido dentro del directorio");
            }

        }

        // Esto es para cargar las instrucciones del fichero script.txt que 
        // se encuentra en el diretorio input/ en el directorio donde se encuentra
        // esta solucion . 
        public static void LoadInstructions()
        {
            Instructions = new Queue<Instruction>();

            var directory = Environment.CurrentDirectory;
            var fileDirectory = Path.Join(directory, "input", "script.txt");

            if (File.Exists(fileDirectory))
            {
                IEnumerable<Instruction> lines = from inst in File.ReadLines(fileDirectory)
                                                 orderby int.Parse(inst.Split(' ')[0]) ascending
                                                 select new Instruction(inst);

                Instructions = new Queue<Instruction>(lines);
            }
            else
            {
                throw new NullReferenceException("no existe el fichero script");
            }
        }

        // Método ejecuta una instrucción en especifico y chequea 
        // que tenga la sintaxis correcta , ante cualquier error esta da una excepción 
        // identificando que pudo haber sucedido
        public static void ExecuteInstruction(Instruction instruction)
        {
            string _instruccion = instruction.AllInstruction;

            string[] startInstruction = _instruccion.Split(" ");

            if (startInstruction.Length < 1)
                ThrowCastException(instruction);

            uint timeInstruction;

            if (!UInt32.TryParse(startInstruction[0], out timeInstruction))
            {
                throw new FormatException($"no tiene un formato válido '{startInstruction[0]}' para ser el tiempo de una instruccion ");
            }

            if (startInstruction.Length < 2)
                ThrowCastException(instruction);

            InstructionType instructionType;

            switch (startInstruction[1])
            {
                case "create":
                    instructionType = InstructionType.create;
                    break;
                case "connect":
                    instructionType = InstructionType.connect;
                    break;
                case "send":
                    instructionType = InstructionType.send;
                    break;
                case "disconnect":
                    instructionType = InstructionType.disconnect;
                    break;
                default:
                    throw new InvalidCastException($" '{startInstruction[1]}' no ese un tipo de instrucción valida");
            }

            if (instructionType == InstructionType.create)
            {
                if (startInstruction.Length < 4)
                    ThrowCastException(instruction);

                string name = startInstruction[3];

                uint portsCount = 1;

                if (startInstruction[2] == "hub")
                {
                    if (startInstruction.Length < 5)
                    {
                        ThrowCastException(instruction);
                    }

                    if (!UInt32.TryParse(startInstruction[4], out portsCount))
                    {
                        throw new FormatException($"La cantidad de puertos '{startInstruction[4]}' de la instrucción no tiene un formato válido");
                    }

                    if (portsCount < 4 || portsCount > 8)
                    {
                        throw new IndexOutOfRangeException("la cantidad de puertos para un hub no son validos");
                    }

                    Hub hub = new Hub(name, (int)portsCount, Devices.Count);
                    Devices.Add(hub);

                }
                else if (startInstruction[2] == "host")
                {
                    portsCount = 1;

                    Computer pc = new Computer(name, Devices.Count);
                    Devices.Add(pc);
                }
            }

            else if (instructionType == InstructionType.connect)
            {
                if (startInstruction.Length < 4)
                    ThrowCastException(instruction);

                string port1 = startInstruction[2];
                string port2 = startInstruction[3];

                Device dev1;
                Device dev2;

                dev1 = Devices.Where(disp => disp.Name.Contains(port1.Split('_').FirstOrDefault())).FirstOrDefault();

                if (dev1 == null)
                {
                    throw new KeyNotFoundException($"No hay ningún dispositivo cuyo nombre sea {port1.Split('_')}");
                }

                dev2 = Devices.Where(disp => disp.Name.Contains(port2.Split('_').FirstOrDefault())).FirstOrDefault();


                if (dev2 == null)
                {
                    throw new KeyNotFoundException($"No hay ningún dispositivo cuyo nombre sea {port2.Split('_')}");
                }

                int numeroport1 = int.Parse(port1.Split('_')[1]) - 1;
                int numeroport2 = int.Parse(port2.Split('_')[1]) - 1;



                Port p1 = dev1.GetPort(numeroport1);
                Port p2 = dev2.GetPort(numeroport2);

                p1.Host = dev2;
                p2.Host = dev1;

                p1.ConnectedPort = port2;
                p2.ConnectedPort = port1;

                p1.IsConnected = true;
                p2.IsConnected = true;

            }

            else if (instructionType == InstructionType.send)
            {
                if (startInstruction.Length < 4)
                    ThrowCastException(instruction);

                string host = startInstruction[2];
                string data = startInstruction[3];

                if (!IsBinaryTheString(data))
                {
                    throw new InvalidCastException($"La información '{data}' que se quiere enviar no tiene un formato correcto ");
                }

                var device = from dev in Devices
                           where dev.Name == host.Split('_')[0]
                           select dev;

                Device[] comp = device.ToArray();
                if (comp.Length != 1)
                {
                    throw new Exception("no se encontró el dispositivo ");
                }

                Computer pc = comp[0] as Computer;

                List<Bit> bitsPackage = new List<Bit>();

                foreach (var item in data)
                {
                    bitsPackage.Add((Bit)int.Parse(item.ToString()));
                }

                pc.send(bitsPackage.ToArray());
            }

            else if (instructionType == InstructionType.disconnect)
            {
                if (startInstruction.Length < 4)
                    ThrowCastException(instruction);

                string port1 = startInstruction[2];
                string port2 = startInstruction[3];

                Device dev1 = Devices.Where(x => x.Name == startInstruction[2].Split('_')[0]).FirstOrDefault();
                Device dev2 = Devices.Where(x => x.Name == startInstruction[3].Split('_')[0]).FirstOrDefault();

                if (dev1 == null)
                    throw new InvalidCastException($"El puerto {port1} al que se esta tratando de acceder no existe ");

                if (dev2 == null)
                    throw new InvalidCastException($"El puerto {port2} al que se esta tratando de acceder no existe ");

                int numeropuerto1 = int.Parse(port1.Split('_')[1]) - 1;
                int numeropuerto2 = int.Parse(port2.Split('_')[1]) - 1;

                Port p1 = dev1.GetPort(numeropuerto1);
                Port p2 = dev2.GetPort(numeropuerto2);

                p1.DisconnectPort();
                p2.DisconnectPort();

            }
        }

        static void ThrowCastException(Instruction instruction)
        {
            throw new InvalidCastException($"La instrucción '{instruction.AllInstruction}' no tiene un formato válido ");
        }

        static bool IsBinaryTheString(string number)
        {
            foreach (var item in number)
            {
                if (item != '0' && item != '1')
                {
                    return false;
                }
            }
            return true;
        }

        // Este enumerable devuelve todas las instrucciones que se va a ejecutar en 
        // un tiempo determinado 
        private static IEnumerable<Instruction> NextInstructionsToExecute(uint time)
        {
            while (Instructions.Count > 0 && Instructions.Peek().Time <= time)
            {
                yield return Instructions.Dequeue();
            }
        }

    }
}
