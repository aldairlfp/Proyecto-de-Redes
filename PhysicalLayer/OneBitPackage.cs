using System.Text;
using PhysicalLayer;

public class OneBitPackage
{
    int time;
    Action action;
    Bit bit;
    ActionResult actionResult;
    string port;

    public OneBitPackage(
        int time,
        Action action,
        Bit bit,
        ActionResult actionResult = ActionResult.None,
        string port = "")
    {
        this.time = time;
        action = action;
        bit = bit;
        actionResult = actionResult;
        this.port = port;
    }

    public int Time
    {
        get => this.time;
    }

    public Bit Bit
    {
        get => this.bit;
    }
    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.Append(this.port);
        stringBuilder.Append(" ");
        stringBuilder.Append(this.time.ToString());
        stringBuilder.Append(" ");
        stringBuilder.Append(this.action.ToString());
        stringBuilder.Append(" ");
        stringBuilder.Append((int)this.bit);
        stringBuilder.Append(" ");
        stringBuilder.Append(this.actionResult == ActionResult.None ? "" : this.actionResult.ToString());
        return stringBuilder.ToString();
    }
}