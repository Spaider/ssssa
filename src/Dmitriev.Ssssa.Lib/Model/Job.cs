using System;
using System.Collections.Generic;

namespace Dmitriev.Ssssa.Model
{
  public class Job
  {
    public Guid          Id    { get; set; }
    public List<JobStep> Steps { get; set; }
  }
}