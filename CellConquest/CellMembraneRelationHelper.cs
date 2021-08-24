using System;
using System.Collections.Generic;
using System.Linq;

namespace CellConquest
{
    public class CellMembraneRelationAlreadyExistsException : Exception
    {
        
    }
    public class CellMembraneRelationHelper
    {
        private List<CellMembraneRelation> relations = new List<CellMembraneRelation>();

        public void AddRelation(Guid cellId, Guid membraneId)
        {
            var relation = new CellMembraneRelation(cellId, membraneId);
            if (relations.Contains(relation))
            {
                throw new CellMembraneRelationAlreadyExistsException();
            }
            relations.Add(new CellMembraneRelation(cellId, membraneId));
        }

        public List<Guid> GetCellIds(Guid membraneId)
        {
            return relations.Where(x => x.MembraneId == membraneId).Select(x => x.CellId).ToList();
        }

        public List<Guid> GetMembraneIds(Guid cellId)
        {
            return relations.Where(x => x.CellId == cellId).Select(x => x.MembraneId).ToList();
        }

        private class CellMembraneRelation
        {
            public Guid CellId { get; }
            public Guid MembraneId { get; }

            public CellMembraneRelation(Guid cellId, Guid membraneId)
            {
                CellId = cellId;
                MembraneId = membraneId;
            }

            private bool Equals(CellMembraneRelation other)
            {
                return CellId.Equals(other.CellId) && MembraneId.Equals(other.MembraneId);
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == this.GetType() && Equals((CellMembraneRelation) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(CellId, MembraneId);
            }
        }
    }
}