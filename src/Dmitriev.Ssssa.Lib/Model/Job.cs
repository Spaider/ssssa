using System;
using System.Collections.Generic;

namespace Dmitriev.Ssssa.Model
{
  public class Job
  {
    public Guid          Id    { get; set; }
    public string        Name  { get; set; }
    public string        Desc  { get; set; }
    public List<JobStep> Steps { get; set; }

    public override string ToString()
    {
      return Name;
    }
  }
}