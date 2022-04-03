using System;
using System.Linq;

namespace AudioEditor
{
    public interface IWaveFormRenderer
    {
        void AddValue(float maxValue, float minValue);
    }
}