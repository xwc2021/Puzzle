public class Tool
{
    public readonly static int NullIndex = -1;

    //找出位在那個小格
    public static int GetIndexOfCell(float x, float cell)
    {
        return (int)((x - (x % cell)) / cell);
    }
}