namespace FloatSoda.Animation;

public interface ISimulation
{
    // TODO:  あまりにも単純すぎる名前なので適切な命名を考える
    double X(double time);

    // TODO: あまりにも単純すぎる名前なので適切な命名を考える
    double Dx(double time);
    bool IsDone(double time);
}

public class InterpolationSimulation(double begin, double end, TimeSpan duration, ICurve curve) : ISimulation
{
    private readonly double _durationInSeconds = duration.TotalSeconds;


    public double X(double time)
    {
        var t = Math.Clamp(time / _durationInSeconds, 0, 1);

        return t switch
        {
            0 => begin,
            1 => end,
            _ => begin + (end - begin) * curve.Transform(t)
        };
    }


    public double Dx(double time)
    {
        const double epsilon = 1e-4;
        return (X(time + epsilon) - X(time - epsilon)) / (2 * epsilon);
    }

    public bool IsDone(double time) => time > _durationInSeconds;
}