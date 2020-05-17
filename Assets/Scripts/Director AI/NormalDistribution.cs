using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalDistribution
{
    private float mean;
    private float deviation;
    public NormalDistribution(float _mean, float _deviation, int seed = -1)
    {
        if (seed != -1)
            UnityEngine.Random.InitState(seed); 
        mean = _mean;
        deviation = _deviation;
    }
    private float PrivateNextGaussian()
    {
        float v1, v2, final;
        do
        {
            v1 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            v2 = 2.0f * Random.Range(0f, 1f) - 1.0f;
            final = v1 * v1 + v2 * v2;
        } while (final >= 1.0f || final == 0f);

        final = Mathf.Sqrt((-2.0f * Mathf.Log(final)) / final);

        return v1 * final;
    }

    public float NextGaussian()
    {
        return mean + PrivateNextGaussian() * deviation;
    }

    public float NextGaussian(float min, float max)
    {
        float rangedValue;
        do
        {
            rangedValue = NextGaussian();
        } while (rangedValue < min || rangedValue > max);
        return rangedValue;
    }
}
