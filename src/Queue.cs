using System.Collections;

namespace Neztu
{
  public interface IQueueManager
  {
    Vote[] GetPlayOrder(Vote[] votes);
  }

  public class RoundRobinQueue : IQueueManager
  {
    public Vote[] GetPlayOrder(Vote[] votes)
    {
      return null;
    }
  }
}
