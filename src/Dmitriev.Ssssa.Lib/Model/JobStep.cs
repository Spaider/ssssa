using System;

namespace Dmitriev.Ssssa.Model
{
  public class JobStep
  {
    public int    Id      { get; set; }
    public Guid   Uid     { get; set; }
    public Guid   JobId   { get; set; }
    public string Name    { get; set; }
    public string SqlCode { get; set; }

    public override string ToString()
    {
      return string.Format("{0}. {1}", Id, Name);
    }
  }
}