namespace Mannerisms.Util;

public static class GeneralUtils
{
    public static bool ValidateMinMax(ref int minV, ref int maxV, int min, int max)
    {
        var modified = false;
        if (minV < min) { minV = min; modified = true; }
        if (maxV > max) { maxV = max; modified = true; }
        if (minV > maxV) { maxV = minV; modified = true; }
        return modified;
    }

    public static bool ValidateMin(ref int minV, int min)
    {
        if (minV < min) {
            minV = min;
            return true;
        }
        return false;
    }

    public static bool ValidateMax(ref int maxV, int max)
    {
        if (maxV > max) {
            maxV = max;
            return true;
        }
        return false;
    }
}
