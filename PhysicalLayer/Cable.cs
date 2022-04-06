namespace PhysicalLayer
{
    public class Cable
    {
        Device dev1;
        Device dev2;

        public Cable(Device dev1, Device dev2)
        {
            this.dev1 = dev1;
            this.dev2 = dev2;
        }

        public Cable() : this(null, null)
        {
        }

        Device Device1
        {
            get => this.dev1;
            set => this.dev1 = value;
        }

        Device Device2
        {
            get => this.dev2;
            set => this.dev2 = value;
        }
    }
}