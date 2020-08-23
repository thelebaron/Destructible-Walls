namespace Prototyping
{
    public struct DNode //: IHandle<DNode>
    {
        //public bool Valid => Handle.Valid && Graph.Valid;

        public bool Equals(DNode other)
        {
            return true; //return Handle.Equals(other.Handle) && Graph.Equals(other.Graph);
        }

        public override bool Equals(object obj)
        {
            //if (ReferenceEquals(null, obj)) return false;
            //return obj is DSPNode other && Equals(other);
            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 1; //return (Handle.GetHashCode() * 397) ^ Graph.GetHashCode();
            }
        }
    }
}