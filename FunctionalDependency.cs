using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchemaKeys
{
    class FunctionalDependency
    {
        private AttributeSet _x;
        private AttributeSet _y;

        public AttributeSet X 
        {
            get { return _x; }
        }
        public AttributeSet Y
        {
            get { return _y; }
        }

        public FunctionalDependency(string x, string y)
        {
            _x = new AttributeSet(x);
            _y = new AttributeSet(y);
        }
        public FunctionalDependency(AttributeSet x, AttributeSet y)
        {
            _x = x;
            _y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is FunctionalDependency)
            {
                return ((FunctionalDependency)obj).GetHashCode() == this.GetHashCode();
            }
            else
            {
                return false;
            }
        }

        public FunctionalDependency GetCopy()
        {
            return new FunctionalDependency(_x.GetCopy(), _y.GetCopy());
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override string ToString()
        {            
            int code = int.Parse("2192", System.Globalization.NumberStyles.HexNumber);
            string unicodeString = char.ConvertFromUtf32(code).ToString();
            return _x.ToString() + " " + unicodeString + " " + _y.ToString();
        }
    }
}
