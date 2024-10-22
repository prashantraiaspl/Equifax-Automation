namespace Equifax.Api.Helper
{
    public class SleepLoader
    {
        public void Seconds(int timeInSeconds)
        {
            System.Threading.Thread.Sleep(timeInSeconds * 1000);
        }
    }
}
