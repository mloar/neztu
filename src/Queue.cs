using System;

namespace Neztu
{
  public interface IQueueManager
  {
    void AddVote(uint UserId, uint TrackId);
    void RemoveVote(uint UserId, uint TrackId);

    uint[] GetQueue();

    uint PlayNext();
  }
}
