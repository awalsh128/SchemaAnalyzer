using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchemaKeys
{
    class Schema
    {
        int _arity;
        String _attributes;
        Dictionary<AttributeSet, AttributeSet> _closures;
        List<FunctionalDependency> _dependencies;
        List<AttributeSet> _candidateKeys;
        List<AttributeSet> _superKeys;

        public static string AllAttributes
        {
            get { return "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
        }
        public string Attributes
        {
            get { return _attributes; }
        }
        public List<AttributeSet> CandidateKeys
        {
            get { return _candidateKeys; }
        }
        public List<AttributeSet> SuperKeys
        {
            get { return _superKeys; }
        }
        public Dictionary<AttributeSet, AttributeSet> Closures
        {
            get { return _closures; }
        }
        public List<FunctionalDependency> Dependencies
        {
            get { return _dependencies; }
        }

        public Schema(int arity)
        {
            _arity = arity;
            _attributes = Schema.AllAttributes.Substring(0, arity);
            _Initialize();
        }
        public Schema(string attributes)
        {
            List<char> sorted = attributes.ToCharArray().Distinct().ToList();
            sorted.Sort();
            _arity = sorted.Count;
            _attributes = String.Join("", sorted);
            _Initialize();
        }
        private void _Initialize()
        {                        
            _candidateKeys = new List<AttributeSet>();
            _closures = new Dictionary<AttributeSet, AttributeSet>();
            _dependencies = new List<FunctionalDependency>();
            _superKeys = new List<AttributeSet>();
            this.Reevaluate();
        }
        
        public void AddDependency(AttributeSet x, AttributeSet y)
        {
            this.AddDependency(new FunctionalDependency(x, y));
        }

        public void AddDependency(string x, string y)
        {
            this.AddDependency(new AttributeSet(x), new AttributeSet(y));
        }

        public void AddDependency(FunctionalDependency item)
        {
            bool closureGrowing = true;

            if (!_dependencies.Contains(item)) _dependencies.Add(item);            
            _closures[item.X].Add(item.Y);

            _EvaluateTransitiveClosure(item.X);
            
            while (closureGrowing)
            {
                closureGrowing = false;
                foreach (KeyValuePair<AttributeSet, AttributeSet> pair in _closures)
                {
                    closureGrowing = closureGrowing || _EvaluateTransitiveClosure(pair.Key);
                }
            }

            _CreateKeys();
        }

        private void _CreateKeys()
        {
            bool isCandidate;

            _candidateKeys.Clear();
            _superKeys.Clear();
            foreach (KeyValuePair<AttributeSet, AttributeSet> pair in _closures)
            {
                if (pair.Value.Count == _arity)
                {                    
                    _superKeys.Add(pair.Key.GetCopy());
                }
            }

            foreach (AttributeSet testSet in _superKeys)
            {
                isCandidate = true;
                foreach (AttributeSet set in _superKeys)                
                {
                    if (testSet.Contains(set) && (testSet != set))
                    {
                        isCandidate = false;
                        break;
                    }
                }
                if (isCandidate) _candidateKeys.Add(testSet.GetCopy());
            }
        }

        private bool _EvaluateTransitiveClosure(AttributeSet x)
        {
            bool closureGrowing = true;     
            bool closureGrew = false;       

            while (closureGrowing)
            {
                closureGrowing = false;
                foreach (KeyValuePair<AttributeSet, AttributeSet> pair in _closures)
                {
                    if (_closures[x].Contains(pair.Key) && !_closures[x].Contains(pair.Value))
                    {
                        _closures[x].Add(pair.Value);
                        closureGrowing = true;
                        closureGrew = true;
                    }
                    
                }
            }

            return closureGrew;
        }

        public bool IsBoyceCoddNormalForm()
        {
            foreach (FunctionalDependency fd in _dependencies)
            {
                // is not a super key and Y is not contained in X
                if ((_closures[fd.X].Count != _arity) && !fd.X.Contains(fd.Y))
                {
                    return false;
                }
            }            
            return true;
        }

        public bool IsThreeNormalForm()
        {
            foreach (FunctionalDependency fd in _dependencies)
            {
                // is not a super key, Y is not contained in X and ...
                if ((_closures[fd.X].Count != _arity) && !fd.X.Contains(fd.Y))
                {
                    bool found = false;
                    // not all attributes of Y are in some candidate key
                    foreach (char attribute in fd.Y.ToString())
                    {
                        foreach (AttributeSet key in _candidateKeys)
                        {
                            if (key.Contains(attribute))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found) return false;
                    }
                }
            }            
            return true;
        }

        public void Reevaluate()
        {
            _closures.Clear();
            foreach (AttributeSet permutation in (new AttributeSet(_attributes)).GetPermutations())
            {
                _closures[permutation] = permutation.GetCopy();
            }
            foreach (FunctionalDependency fd in _dependencies)
            {
                this.AddDependency(fd.X, fd.Y);
            }
        }
    }
}