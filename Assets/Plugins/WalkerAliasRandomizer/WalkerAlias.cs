using System;
using System.Collections.Generic;
using System.Linq;

namespace WalkerAliasRandomizer
{
    public class WalkerAlias<T>
    {
        private readonly List<int> _alias;
        private readonly List<double> _probabilities;
        private readonly Random _random = new Random();
        // private IList<KeyValuePair<T, int>> _values;
        private IList<KeyValuePair<T, double>> _values;
        private int _weightLength;

        private int[] alias;
        private double[] probability;

        public List<int> Alias => _alias;
        public List<double> Probabilities => _probabilities;

        public WalkerAlias()
        {
            _alias = new List<int>();
            _probabilities = new List<double>();
        }

        public void Build(IList<KeyValuePair<T, double>> values)
        {
            var underFull = new List<int>();
            var overFull = new List<int>();

            _weightLength = values.Count;
            _values = values;

            probability = new double[_weightLength];
            alias = new int[_weightLength];

            var cumulativeSum = values.Select(x => x.Value).Sum();

            PopulateProbabilityAndAlias(cumulativeSum);

            PopulateOverAndUnderfull(underFull, overFull);

            ProcessOverAndUnderfull(underFull, overFull);
        }

        private void PopulateProbabilityAndAlias(double cumulativeSum)
        {
            foreach (var value in _values)
            {
                _alias.Add(-1);

                var probability = (double)value.Value * _weightLength / cumulativeSum;

                _probabilities.Add(probability);
            }
        }

        private void PopulateOverAndUnderfull(List<int> underFull, List<int> overFull)
        {
            for (var i = 0; i < _probabilities.Count(); i++)
            {
                if (_probabilities[i] < 1)
                    underFull.Add(i);
                if (_probabilities[i] > 1)
                    overFull.Add(i);
            }
        }

        private void ProcessOverAndUnderfull(IList<int> underFull, IList<int> overFull)
        {
           // while (underFull.Any() && overFull.Any())
            while (underFull.Any() && overFull.Any())
            {
                var currentUnder = underFull.Pop();
                //var currentOver = overFull.Last();

                //_alias[currentUnder] = currentOver;
                //_probabilities[currentOver] -= (1 - _probabilities[currentUnder]);

                //if (_probabilities[currentOver] < 1)
                //{
                //    underFull.Add(currentOver);
                //    overFull.Pop();
                //}
            }
        }

        public T GetSelection()
        {
            if (!_probabilities.Any() || !_alias.Any())
            {
                throw new InvalidOperationException("Weights have not been set. Build method must be called before a selection is made.");
            }

            var fairDiceRoll = _random.NextDouble();
            var biasedCoin = GetBiasedRandom(0, _weightLength - 1);

            var selectionIndex = _probabilities[biasedCoin] >= fairDiceRoll
                                 ? biasedCoin
                                 : _alias[biasedCoin];

            return _values[selectionIndex].Key;
        }

        private int GetBiasedRandom(int min, int max)
        {
            return Convert.ToInt32(Math.Floor(_random.NextDouble() * (max - min + 1)) + min);
        }
    }
}
