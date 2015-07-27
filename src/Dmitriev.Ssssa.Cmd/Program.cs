using System;
using System.Configuration;
using Dmitriev.Ssssa.Model;

namespace Dmitriev.Ssssa.Cmd
{
  class Program
  {
    private static JobManager _jobManager; 
    private static string     _connString;

    static int Main(string[] args)
    {
      _connString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
      _jobManager = new JobManager(_connString);

      ShowBanner();
      ShowConnectionInfo();

      if (args.Length == 0 || args[0].ToLower() == "list")
      {
        PrintJobSummary();
        return 0;
      }

      Guid jobId;
      if (!Guid.TryParse(args[0], out jobId))
      {
        Console.WriteLine("'{0}' is not valid GUID");
        return 1;
      }

      _jobManager.ExecJob(jobId);
      return 0;
    }

    private static void PrintJobSummary()
    {
      var jobs = _jobManager.GetAllJobs();

      foreach (var job in jobs)
      {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(job.Id + " ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(job.Name);
      }
      Console.WriteLine();
    }

    private static void ShowBanner()
    {
      Console.WriteLine();
      Console.WriteLine("Simple Standalone SQL Server Agent");
      Console.WriteLine("Created by Denis Dmitriev, 2015");
      Console.WriteLine();
    }

    private static void ShowConnectionInfo()
    {
      Console.ForegroundColor = ConsoleColor.DarkGreen;
      Console.Write("Connection: ");
      Console.ForegroundColor = ConsoleColor.Gray;
      Console.WriteLine(_connString);
      Console.WriteLine();
    }
  }
}
