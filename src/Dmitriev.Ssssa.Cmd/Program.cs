using System.Configuration;
using Dmitriev.Ssssa.Model;

namespace Dmitriev.Ssssa.Cmd
{
  class Program
  {
    static void Main(string[] args)
    {
      var mgr = new JobManager(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
      var jobs = mgr.GetAllJobs();
    }
  }
}
