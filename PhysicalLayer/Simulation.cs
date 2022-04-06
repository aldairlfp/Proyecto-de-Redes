using System.Collections.Generic;

namespace PhysicalLayer
{
    public class Simulation
    {
        // Mili-segundo actual , por el que va ejecutándose el programa
        public static uint CurrentTime = 0;

        // Tiempo que demora un bit transmitiéndose por un canal 
        public static uint SignalTime = 10;

        // Tiempo máximo que puede corres el programa (la medida de tiempo es como mili-segundos)
        public static uint MaxTime = 1000000;

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

        public Simulation()
        {
            RunAplication();
        }

        public void RunAplication()
        {
            Devices = new List<Device>();

            //Esto limpia el directorio de la salida (es decir borra todos los ficheros que hay 
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
            Configurar();


            //Este es el ciclo principal para correr las instrucciones y hacer el envió de 
            //información entre todos los host que están conectados.
            while (CurrentTime < MaxTime)
            {

                Console.WriteLine($"CURRENT TIME : {CurrentTime} mili-second");


                //Ejecutar las instrucciones que corresponden a ejecutarse en el 
                //mili-segundo actual que están en la cola de instrucciones ; 
                foreach (var item in NextInstructionsToExecute(CurrentTime))
                {
                    ExecuteInstruction(item);
                }

                //Actualizar el bit de salida de cada computadora para después 
                //enviar el bit que esta en la salida a cada uno de los 
                //Dispositivos a los que esta conectado la Computadora
                foreach (var item in Devices.Where(e => e is Computer))
                {
                    Computer? comp = item as Computer;
                    comp.SendOutputBitToOtherComputer();
                }

                //verifica las entrada por cada dispositivo y chequea su hubo 
                //una colisión, y escribe en la salida (en su txt correspondiente )
                //la salida que este dispositivo tiene. 
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
        public void SetDefaultConfig()
        {
            MinPortInHub = 4;
            MaxPortInHub = 8;
            SignalTime = 10;
            MaxTime = 1000000;
        }

        // Lee del fichero config.txt que se 
        // encuentra en este mismo directorio en que esta este proyecto 
        // este pone todos los parametros establecido que se encuentran en
        // el fichero 
        public void Config()
        {
            var CurrentDirectory = Environment.CurrentDirectory;
            var parent = Directory.GetParent(Directory.GetParent(Directory.GetParent(CurrentDirectory).FullName).FullName);

            string fullDirectory = Path.Join(parent.FullName, "config.txt");

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

                string[] configuracionpartida = item.Split(':');

                if (configuracionpartida.Length < 2) throw new InvalidCastException($"la configuración {item} no tiene el formato correcto");

                switch (configuracionpartida[0])
                {
                    case "max_cantidad_milisegundos":
                        UInt32 _tiempo_maximo;
                        if (UInt32.TryParse(configuracionpartida[1], out _tiempo_maximo))
                        {
                            Program.tiempo_maximo = _tiempo_maximo;
                        }
                        else
                        {
                            throw new InvalidCastException($"el numero para asignarle al tiempo_maximo: '{configuracionpartida[1]} no es valido '");
                        }
                        break;

                    case "signal_time":
                        int _signal_time;
                        if (Int32.TryParse(configuracionpartida[1], out _signal_time))
                        {
                            Program.signal_time = (uint)_signal_time;
                        }
                        else
                        {
                            throw new InvalidCastException($"el numero para asignarle el _signal_time '{configuracionpartida[1]}' no tiene el formato correcto");
                        }
                        break;
                    case "numero_puertos_hub":
                        string[] extremosdelintervalo = configuracionpartida[1].Split('-');

                        if (extremosdelintervalo.Length < 2)
                        {
                            throw new InvalidCastException($"No tiene el formato correcto los intervalos '{extremosdelintervalo}'");
                        }

                        int min, max;

                        if (!int.TryParse(extremosdelintervalo[0], out min))
                        {
                            throw new InvalidCastException($"El extremo {extremosdelintervalo[0]} no es un numero valido ");
                        }
                        if (!int.TryParse(extremosdelintervalo[1], out max))
                        {
                            throw new InvalidCastException($"El extremo {extremosdelintervalo[1]} no es un numero valido ");
                        }

                        if (sonvalidoslacantidaddepuertosdeunhub(min, max))
                        {
                            Program.cantidadminimadepuertosdeunhub = min;
                            Program.cantidadmaximadepuertosdeunhub = max;
                        }
                        break;

                }
            }
        }

        public static bool sonvalidoslacantidaddepuertosdeunhub(int a, int b)
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


        /// <summary>
        /// esto borra todos los ficheros que se encuentran en el directorio 
        /// output y se llama al principio de la ejecución del programa , antes de 
        /// entrar en el ciclo principal , y se hace para barrar todos los fichero que 
        /// se podían haber generado previamente en la ejecución de programa en un momento 
        /// anterior
        /// </summary>
        public void CleanOutputDirectory()
        {
            var CurrentDirectory = Environment.CurrentDirectory;
            var parent = Directory.GetParent(Directory.GetParent(Directory.GetParent(CurrentDirectory).FullName).FullName);

            var salida = Path.Join(parent.FullName, "output");

            if (Directory.Exists(salida))
            {
                foreach (var item in Directory.GetFiles(salida))
                {
                    File.Delete(item);
                }
            }
            else
            {
                throw new Exception("La Salida no existe , para borrar el contenido dentro del directorio");
            }

        }

        /// <summary>
        /// Esto es para cargar las instrucciones del fichero script.txt que 
        /// se encuentra en el diretorio input/ en el directorio donde se encuentra
        /// esta solucion . 
        /// </summary>
        public  void LoadInstructions()
        {
            Instructions = new Queue<Instruction>();

            var directory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName);
            var directoriodelfichero = Path.Join(directory.FullName, "input", "script.txt");

            if (File.Exists(directoriodelfichero))
            {
                IEnumerable<Instruction> lines = from inst in File.ReadLines(directoriodelfichero)
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
            string _instruccion = instruction.AllInstruccion;

            string[] instruccionpartida = _instruccion.Split(" ");

            if (instruccionpartida.Length < 1)
                LanzarExepciondeCasteo(instruccion);

            uint tiempodelainstruccion;

            if (!UInt32.TryParse(instruccionpartida[0], out tiempodelainstruccion))
            {
                throw new FormatException($"no tiene un formato válido '{instruccionpartida[0]}' para ser el tiempo de una instruccion ");
            }

            if (instruccionpartida.Length < 2)
                LanzarExepciondeCasteo(instruccion);

            TipodeInstruccion tipoinstruccion;

            switch (instruccionpartida[1])
            {
                case "create":
                    tipoinstruccion = TipodeInstruccion.create;
                    break;
                case "connect":
                    tipoinstruccion = TipodeInstruccion.connect;
                    break;
                case "send":
                    tipoinstruccion = TipodeInstruccion.send;
                    break;
                case "disconnect":
                    tipoinstruccion = TipodeInstruccion.disconnect;
                    break;
                default:
                    throw new InvalidCastException($" '{instruccionpartida[1]}' no ese un tipo de instrucción valida");
            }

            if (tipoinstruccion == TipodeInstruccion.create)
            {
                if (instruccionpartida.Length < 4)
                    LanzarExepciondeCasteo(instruccion);

                string name = instruccionpartida[3];

                uint cantidaddepuertos = 1;

                if (instruccionpartida[2] == "hub")
                {
                    if (instruccionpartida.Length < 5)
                    {
                        LanzarExepciondeCasteo(instruccion);
                    }

                    if (!UInt32.TryParse(instruccionpartida[4], out cantidaddepuertos))
                    {
                        throw new FormatException($"La cantidad de puertos '{instruccionpartida[4]}' de la instrucción no tiene un formato válido");
                    }

                    if (cantidaddepuertos < 4 || cantidaddepuertos > 8)
                    {
                        throw new IndexOutOfRangeException("la cantidad de puertos para un hub no son validos");
                    }

                    Hub hub = new Hub(name, (int)cantidaddepuertos, Program.dispositivos.Count);
                    Program.dispositivos.Add(hub);

                }
                else if (instruccionpartida[2] == "host")
                {
                    cantidaddepuertos = 1;

                    Computadora computadora = new Computadora(name, Program.dispositivos.Count);
                    Program.dispositivos.Add(computadora);
                }
            }

            else if (tipoinstruccion == TipodeInstruccion.connect)
            {
                if (instruccionpartida.Length < 4)
                    LanzarExepciondeCasteo(instruccion);

                string port1 = instruccionpartida[2];
                string port2 = instruccionpartida[3];

                Dispositivo disp1;
                Dispositivo disp2;

                disp1 = dispositivos.Where(disp => disp.Name.Contains(port1.Split('_').FirstOrDefault())).FirstOrDefault();

                if (disp1 == null)
                {
                    throw new KeyNotFoundException($"No hay ningún dispositivo cuyo nombre sea {port1.Split('_')}");
                }

                disp2 = dispositivos.Where(disp => disp.Name.Contains(port2.Split('_').FirstOrDefault())).FirstOrDefault();


                if (disp2 == null)
                {
                    throw new KeyNotFoundException($"No hay ningún dispositivo cuyo nombre sea {port2.Split('_')}");
                }

                int numeroport1 = int.Parse(port1.Split('_')[1]) - 1;
                int numeroport2 = int.Parse(port2.Split('_')[1]) - 1;



                Puerto p1 = disp1.DameElPuerto(numeroport1);
                Puerto p2 = disp2.DameElPuerto(numeroport2);

                p1.DispositivoConectado = disp2;
                p2.DispositivoConectado = disp1;

                p1.PuertoAlQueEstaConnectado = port2;
                p2.PuertoAlQueEstaConnectado = port1;

                p1.EstaConectadoAOtroDispositivo = true;
                p2.EstaConectadoAOtroDispositivo = true;

            }

            else if (tipoinstruccion == TipodeInstruccion.send)
            {
                if (instruccionpartida.Length < 4)
                    LanzarExepciondeCasteo(instruccion);

                string host = instruccionpartida[2];
                string data = instruccionpartida[3];

                if (!Esbinarioelstring(data))
                {
                    throw new InvalidCastException($"La información '{data}' que se quiere enviar no tiene un formato correcto ");
                }

                var disp = from dispositivo in dispositivos
                           where dispositivo.Name == host.Split('_')[0]
                           select dispositivo;

                Dispositivo[] comp = disp.ToArray();
                if (comp.Length != 1)
                {
                    throw new Exception("no se encontró el dispositivo ");
                }

                Computadora computadora = comp[0] as Computadora;

                List<Bit> paquetedebits = new List<Bit>();

                foreach (var item in data)
                {
                    paquetedebits.Add((Bit)int.Parse(item.ToString()));
                }

                computadora.send(paquetedebits.ToArray());
            }

            else if (tipoinstruccion == TipodeInstruccion.disconnect)
            {
                if (instruccionpartida.Length < 4)
                    LanzarExepciondeCasteo(instruccion);

                string port1 = instruccionpartida[2];
                string port2 = instruccionpartida[3];

                Dispositivo dispositivo1 = dispositivos.Where(x => x.Name == instruccionpartida[2].Split('_')[0]).FirstOrDefault();
                Dispositivo dispositivo2 = dispositivos.Where(x => x.Name == instruccionpartida[3].Split('_')[0]).FirstOrDefault();

                if (dispositivo1 == null)
                    throw new InvalidCastException($"El puerto {port1} al que se esta tratando de acceder no existe ");

                if (dispositivo2 == null)
                    throw new InvalidCastException($"El puerto {port2} al que se esta tratando de acceder no existe ");

                int numeropuerto1 = int.Parse(port1.Split('_')[1]) - 1;
                int numeropuerto2 = int.Parse(port2.Split('_')[1]) - 1;

                Puerto p1 = dispositivo1.DameElPuerto(numeropuerto1);
                Puerto p2 = dispositivo2.DameElPuerto(numeropuerto2);

                p1.DesconectarElPuerto();
                p2.DesconectarElPuerto();

            }

        }

        static void LanzarExepciondeCasteo(Instruccion instruccion)
        {
            throw new InvalidCastException($"La instrucción '{instruccion.instruccion}' no tiene un formato válido ");
        }

        static bool Esbinarioelstring(string numero)
        {
            foreach (var item in numero)
            {
                if (item != '0' && item != '1')
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Este enumerable devuelve todas las instrucciones que se va a ejecutar en 
        /// un tiempo determinado 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private static IEnumerable<Instruccion> NextInstructionsToExecute(uint time)
        {
            while (Program.instrucciones.Count > 0 && Program.instrucciones.Peek().Time <= time)
            {
                yield return Program.instrucciones.Dequeue();
            }
        }

    }
}
