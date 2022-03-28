namespace PhysicalLayer;

public class Instruccion
{
    int time;
    string instruccion;
    public Instruccion(string instruccion)
    {
        this.instruccion = instruccion;
        int.TryParse(instruccion.Split(' ')[0], out time);
    }

    public String instruccion
    {
        get => this.instruccion;
    }

    public int Time
    {
        get => this.time;
    }
}