namespace PhysicalLayer;

public enum Bit
    {
        Zero = 0,
        One = 1,
        None = 2
    }

public enum Action
    {
        Received = 0,
        Send = 1
    }

public enum ActionResult
    {
       Ok=0, 
       Received=1,
       Collision=2,
       None=3, 
    }