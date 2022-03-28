namespace PhysicalLayer;

public class Cable
{
    Port _puerto1;
    Port _puerto2;

    Bit _bit1;
    Bit _bit2;


    public Cable()
    {
        this._bit1 = Bit.none;
        this._bit2 = Bit.none;
    }

    public Port puerto1 { get => this._puerto1; set => this._puerto1 = value; }
    public Port puerto2 { get => this._puerto2; set => this._puerto2 = value; }
    public Bit bit1 { get => this._bit1; set => this._bit1 = value; }
    public Bit bit2 { get => Bit.none; set => this._bit2 = Bit.none; }
}
