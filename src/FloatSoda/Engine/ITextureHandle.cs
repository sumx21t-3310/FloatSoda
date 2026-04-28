namespace FloatSoda.Engine;

public interface ITextureHandle
{
    public IntPtr GetHandle();
    public void Flush();
}