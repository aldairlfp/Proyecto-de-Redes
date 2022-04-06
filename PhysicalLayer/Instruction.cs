namespace PhysicalLayer
{
    public class Instruction
    {
        // Tiempo en el que le toca ejecutarse la instrucion 
        uint time;

        // String completo donde esta toda la informacion que
        // tiene la instruccion 
        public string AllInstruction { get; private set; }

        // Este es el constructor
        // aquí pone automáticamente en la variable time 
        // el valor del entero que representa el tiempo en el 
        // que le corresponde ejecutarse la instrucción 
        public Instruction(string instruction)
        {
            AllInstruction = instruction;
            uint.TryParse(instruction.Split(' ')[0], out time);
        }

        public uint Time => this.time;
    }
}
