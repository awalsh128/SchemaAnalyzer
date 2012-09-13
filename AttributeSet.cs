using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchemaKeys
{
    class AttributeSet
    {
        private List<char> _attributes;

        public int Count
        {
            get { return _attributes.Count; }
        }
        
        public AttributeSet(string attributes)
        {
            _attributes = new List<char>();
            _attributes.AddRange(attributes.ToCharArray());
            _attributes.Sort();
        }

        public bool Contains(char attribute)
        {
            if (!_attributes.Contains(attribute))
            {
                return false;            
            }
            else
            {
                return true;
            }            
        }
        public bool Contains(AttributeSet set)
        {
            foreach (char attribute in set._attributes)
            {
                if (!_attributes.Contains(attribute)) return false;
            }
            return true;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is AttributeSet)
            {
                return ((AttributeSet)obj).GetHashCode() == this.GetHashCode();
            }
            else
            {
                return false;
            }
        }

        public AttributeSet GetCopy()
        {
            return new AttributeSet(String.Join("", _attributes));
        }

        public override int GetHashCode()
        {
            return String.Join("", _attributes).GetHashCode();
        }

        public List<AttributeSet> GetPermutations()
        {
            int permutationCount = (int)Math.Pow(2, _attributes.Count);
            BitArray permuteFlags;
            List<char> permutation;
            List<AttributeSet> permutations = new List<AttributeSet>();
            for (int i = 1; i < permutationCount; i++)
            {
                permutation = new List<char>();
                permuteFlags = new BitArray(System.BitConverter.GetBytes(i));
                for (int j = 0; j < _attributes.Count; j++)
                {
                    if (permuteFlags[j]) permutation.Add(_attributes[j]);
                }
                permutations.Add(new AttributeSet(string.Join("", permutation)));
            }

            return permutations;
        }

        public override string ToString()
        {
            return String.Join("", _attributes);
        }

        public void Add(char attribute)
        {
            if (!_attributes.Contains(attribute)) _attributes.Add(attribute);
            _attributes.Sort();
        }

        public void Add(AttributeSet set)
        {
            foreach (char attribute in set._attributes)
            {
                if (!_attributes.Contains(attribute)) _attributes.Add(attribute);
            }
            _attributes.Sort();
        }

    }
}
