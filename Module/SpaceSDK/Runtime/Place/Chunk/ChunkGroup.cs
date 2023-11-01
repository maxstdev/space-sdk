using System.Collections.Generic;

namespace MaxstXR.Place
{
    public abstract class AbstractGroup : IEqualityComparer<AbstractGroup>
    { 
        public AbstractGroup()
        {

        }

        public abstract long GetGroupId();

        public bool Equals(AbstractGroup x, AbstractGroup y)
        {
            return x.GetGroupId() == y.GetGroupId();
        }

        public int GetHashCode(AbstractGroup obj)
        {
            return obj.GetGroupId().GetHashCode();
        }

        public override int GetHashCode()
        {
            return GetGroupId().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is AbstractGroup other)
            {
                return Equals(this, other);
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return GetGroupId().ToString();
        }
    }

    public class ChunkGroup : AbstractGroup
    {
        public long GroupId { get; private set; } = -1L;

        public ChunkGroup(long id) : base()
        {
            GroupId = id;
        }

        public override long GetGroupId()
        {
            return GroupId;
        }
    }
}
