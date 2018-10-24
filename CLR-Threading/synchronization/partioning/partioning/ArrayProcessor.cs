using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace partioning
{
    class ArrayProcessor
    {
        double[] _data;
        int _firstIndex;
        int _lastIndex;
        double _sum;

        public ArrayProcessor(double[] data, int firstIndex, int lastIndex)
        {
            _data = data;
            _firstIndex = firstIndex;
            _lastIndex = lastIndex;
        }

        public void ComputeSum()
        {
            _sum = 0;

            for (int n = _firstIndex; n <= _lastIndex; n++)
            {
                _sum += _data[n];
                Thread.Sleep(1);
            }
        }

        public double Sum
        {
            get { return (_sum); }
        }
    }

}
