using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Dmitriev.Ssssa.Model
{
  public class JobManager
  {
    private readonly string _connString;

    public JobManager(string connString)
    {
      if (string.IsNullOrWhiteSpace(connString))
        throw new ArgumentNullException("connString", "Valid connection string required");
      
      _connString = connString;
    }

    #region Public methods

    public List<Job> GetAllJobs()
    {
      var jobs = GetJobsInternal(Guid.Empty);
      var steps = GetStepsInternal(Guid.Empty);

      foreach (var job in jobs)
      {
        job.Steps = 
          (from st in steps
          where st.JobId == job.Id
          orderby st.Id
          select st).ToList();
      }
      return jobs;
    }

    public Job GetJob(Guid jobId)
    {
      if (jobId == Guid.Empty)
        throw new ArgumentException("Job ID cannot be empty", "jobId");

      var job = GetJobsInternal(jobId).FirstOrDefault();
      if (job == null)
      {
        return null;
      }
      job.Steps = GetStepsInternal(jobId);
      return job;
    }

    public void ExecJob(Guid jobId)
    {
      if (jobId == Guid.Empty)
      {
        throw new ArgumentException("Job ID must be specified", "jobId");
      }

      var job = GetJob(jobId);
      if (job == null)
      {
        throw new ApplicationException(string.Format("Job ID {0} not found", jobId));
      }
      if (job.Steps == null || !job.Steps.Any())
      {
        Console.WriteLine("Job contains not steps");
        return;
      }
      Console.WriteLine("Executing job '{0}'", job.Name);

      using (var conn = new SqlConnection(_connString))
      {
        conn.Open();
        conn.InfoMessage += (sender, args) =>
                  {
                    Console.WriteLine(args.Message);
                  };
        foreach (var step in job.Steps)
        {
          Console.ForegroundColor = ConsoleColor.DarkGreen;
          Console.WriteLine("Step {0}: {1}", step.Id, step.Name);
          Console.ForegroundColor = ConsoleColor.Gray;
          
          var cmd = new SqlCommand(step.SqlCode, conn);
          cmd.ExecuteNonQuery();
        }
        conn.Close();
      }
      Console.ForegroundColor = ConsoleColor.DarkGreen;
      Console.WriteLine("Job completed");
      Console.ForegroundColor = ConsoleColor.Gray;
    }

    #endregion

    #region Private methods

    private List<Job> GetJobsInternal(Guid jobId)
    {
      var jobs = new List<Job>();
      using (var conn = new SqlConnection(_connString))
      {
        conn.Open();
        var cmd =
          jobId == Guid.Empty
            ? new SqlCommand("SELECT ov.job_id, ov.name, ov.[description] FROM msdb.dbo.sysjobs_view ov ORDER BY name", conn)
            : new SqlCommand("SELECT ov.job_id, ov.name, ov.[description] FROM msdb.dbo.sysjobs_view ov WHERE ov.job_id = '" + jobId + "' ORDER BY name", conn);
        using (var reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            var job = new Job
            {
              Id = (Guid) reader["job_id"],
              Desc = Convert.ToString(reader["description"]),
              Name = Convert.ToString(reader["name"])
            };
            jobs.Add(job);
          }
        }
        conn.Close();
      }
      return jobs;
    }

    private List<JobStep> GetStepsInternal(Guid jobId)
    {
      var steps = new List<JobStep>();
      using (var conn = new SqlConnection(_connString))
      {
        conn.Open();  
        var cmd =
          jobId == Guid.Empty
            ? new SqlCommand("SELECT s.job_id, s.step_id, s.step_uid, s.step_name, s.command FROM msdb.dbo.sysjobsteps AS s WHERE s.subsystem = 'TSQL'", conn)
            : new SqlCommand("SELECT s.job_id, s.step_id, s.step_uid, s.step_name, s.command FROM msdb.dbo.sysjobsteps AS s WHERE s.subsystem = 'TSQL' and s.job_id='" + jobId + "'", conn);
        using (var reader = cmd.ExecuteReader())
        {
          while (reader.Read())
          {
            var step = new JobStep
            {
              JobId = (Guid)reader["job_id"],
              Id = Convert.ToInt32(reader["step_id"]),
              Uid = (Guid)reader["step_uid"],
              SqlCode = Convert.ToString(reader["command"]),
              Name = Convert.ToString(reader["step_name"])
            };
            steps.Add(step);
          }
        }
        conn.Close();
      }
      return steps;      
    }

    #endregion
  }
}