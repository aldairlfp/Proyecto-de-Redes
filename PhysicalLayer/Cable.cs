public class Cable
    {
        public Cable(Device dev1, Device dev2)
            {
            device1 = dev1;
            device2 = dev2; 
            }

        public Cable (): this(null , null )
        {
        }

        Device dev1;
        Device dev2;

        Device Device1
        {
            get => this.dev1;
            set => this.dev1 = value;
        }

        Dispositivo Device2
        {
            get => this.dev2;
            set => this.dev2 = value;
        }
    }
