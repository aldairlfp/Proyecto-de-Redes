namespace PhysicalLayer
{
    public class Hub : Device
    {
        // Este es el constructor por defecto 
        // siempre asume que el Hub va a tener 4 puertos
        // 
        // El parámetro indice es para saber que indice tiene el 
        // dispositivo en el montículo donde se almacenan los
        // dispositivos en general. 
        public Hub(string name, int indice) : base(name, 4, indice)
        {
        }

        // Este es el constructor que chequea que la cantidad de
        // puertos con que se va a crear el hub tenga una cantidad 
        // de puertos que este en el rango de la configuración de la 
        // cantidad de puertos establecidas
        public Hub(string name, int numerodepuertos, int indice) : base(name, numerodepuertos, indice)
        {
            if (numerodepuertos < Simulation.MinPortInHub || numerodepuertos > Simulation.MaxPortInHub)
            {
                throw new IndexOutOfRangeException("No se puede tener un hub con menos de 4 puertos o mas de 8 ");
            }
        }
    }
}