using System;

namespace CellConquest
{
    
    public class Membrane
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string TouchedBy { get; private set; } = StaticGameValues.NoOne;

        public void MarkAsTouchedBy(string name)
        {
            if (TouchedBy != StaticGameValues.NoOne)
            {
                throw new MembraneAlreadyTouchedException();
            }

            TouchedBy = name;
        }
    }
    public class MembraneAlreadyTouchedException : Exception
    {
    }
}