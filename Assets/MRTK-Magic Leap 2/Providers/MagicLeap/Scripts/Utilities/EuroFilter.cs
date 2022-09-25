using System.Linq;
using UnityEngine;

namespace MagicLeap.MRTK.Utilities
{
    public class EuroFilter
    {
        public float[] DerivativeCutoff { get; set; }
        public float[] MinCutoff { get; set; }
        public float[] Beta { get; set; }

        private float _deltaTime;

        private float[] _delta;
        private float[] _result;

        private float[] _previousData;
        private float[] _previousDelta;
        private float _previousTimestamp;

        private bool _initialized;
        private readonly bool _resetOnZero;
        private int _order;

        public EuroFilter(int order, float[] minCutoff, float[] beta, float[] derivativeCutoff, bool resetOnZero = true)
        {
            _resetOnZero = resetOnZero;
            _deltaTime = 0;
            _order = order;
            _initialized = false;

            DerivativeCutoff = derivativeCutoff;
            MinCutoff = minCutoff;
            Beta = beta;

            _delta = new float[order];
            _result = new float[order];

            _previousData = new float[order];
            _previousDelta = new float[order];
        }

        public EuroFilter(int order, float minCutoff, float beta, float derivativeCutoff, bool resetOnZero = true)
        {
            _resetOnZero = resetOnZero;
            _deltaTime = 0;
            _order = order;
            _initialized = false;

            DerivativeCutoff = Enumerable.Repeat(derivativeCutoff, order).ToArray();
            MinCutoff = Enumerable.Repeat(minCutoff, order).ToArray();
            Beta = Enumerable.Repeat(beta, order).ToArray();

            _delta = new float[order];
            _result = new float[order];

            _previousData = new float[order];
            _previousDelta = new float[order];
        }

        public void UpdateFilter(int order, float[] minCutoff, float[] beta, float[] derivativeCutoff)
        {
            _order = order;
            MinCutoff = minCutoff;
            Beta = beta;
            DerivativeCutoff = derivativeCutoff;
        }

        public void UpdateFilter(int order, float minCutoff, float beta, float derivativeCutoff)
        {
            var minCutoffs = Enumerable.Repeat(minCutoff, order).ToArray();
            var betas = Enumerable.Repeat(beta, order).ToArray();
            var derivativeCutoffs = Enumerable.Repeat(derivativeCutoff, order).ToArray();
            UpdateFilter(order, minCutoffs, betas, derivativeCutoffs);
        }

        public Vector3 Filter(float timestamp, Vector3 vector)
        {
            var filteredVector = Filter(timestamp, new[] { vector.x, vector.y, vector.z });
            if (_initialized)
                return new Vector3(filteredVector[0], filteredVector[1], filteredVector[2]);
            else
            {
                return vector;
            }
        }

        public Quaternion Filter(float timestamp, Quaternion quaternion)
        {
            var filteredQuaternion =
                Filter(timestamp, new[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w });

            if (_initialized)
            {
                return new Quaternion(filteredQuaternion[0], filteredQuaternion[1], filteredQuaternion[2],
                                      filteredQuaternion[3]);
            }
            else
            {
                return quaternion;
            }
           
        }

        public float[] Filter(float timestamp, float[] dataPoint)
        {
            if (_resetOnZero && dataPoint.All(x => x == 0f) || !IsValid(dataPoint))
            {
                Reset();
                return dataPoint;
            }

            if (_previousTimestamp != 0 && timestamp != 0)
            {
                _deltaTime = timestamp - _previousTimestamp;
            }

            _previousTimestamp = timestamp;

            if (!_initialized)
            {
                _previousData = dataPoint;
            }

            for (var i = 0; i < _order; ++i)
            {
                _delta[i] = _initialized ? (dataPoint[i] - _previousData[i]) / _deltaTime : 0f;

                var derivativeSmoothFactor = GetSmoothingFactor(DerivativeCutoff[i]);
                var dxHat = ExponentialSmoothing(derivativeSmoothFactor, _delta[i], _previousDelta[i]);

                var cutoff = MinCutoff[i] + Beta[i] * Mathf.Abs(dxHat);
                var smoothFactor = GetSmoothingFactor(cutoff);
                _result[i] = ExponentialSmoothing(smoothFactor, dataPoint[i], _previousData[i]);
                _previousData[i] = _result[i];
                _previousDelta[i] = dxHat;
            }
    
            _initialized = true;
            if (!IsValid(_result))
            {
                Reset();
                return dataPoint;
            }
            return _result;
        }

        private bool IsValid(float[] dataPoint)
        {
            for (int i = 0; i < dataPoint.Length; i++)
            {
                if (double.IsNaN(dataPoint[i]) || double.IsInfinity(dataPoint[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private float GetSmoothingFactor(float cutoff)
        {
            var result = 2f * Mathf.PI * cutoff * _deltaTime;
            return result / (result + 1f);
        }

        private float ExponentialSmoothing(float smoothFactor, float value, float previousValue)
        {
            return smoothFactor * value + (1f - smoothFactor) * previousValue;
        }

        public void Reset()
        {
            _initialized = false;
            _delta = new float[_order];
            _result = new float[_order];

            _previousData = new float[_order];
            _previousDelta = new float[_order];
            _previousTimestamp = 0f;
        }
    }
}