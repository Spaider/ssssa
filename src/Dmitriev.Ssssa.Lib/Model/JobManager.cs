using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Dmitriev.Ssssa.Model
{
  public class JobManager
  {
    private string _connString;

    public JobManager(string connString)
    {
      if (string.IsNullOrWhiteSpace(connString))
        throw new ArgumentNullException("connString", "Valid connection string required");
      
      _connString = connString;
    }

    #region Public methods

    public List<Job> GetAllJobs()
    {
      var jobs = GetAllJobsInternal();
      var steps = GetAllStepsInternal();

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

    public List<Job> GetJob(Guid jobId)
    {
      if (jobId == Guid.Empty)
        throw new ArgumentException("Job ID cannot be empty", "jobId");

      return null;
    }

    #endregion

    #region Private methods

    private List<Job> GetAllJobsInternal()
    {
      var jobs = new List<Job>();
      using (var conn = new SqlConnection(_connString))
      {
        conn.Open();
        var cmd =
          new SqlCommand("SELECT ov.job_id, ov.name, ov.[description] FROM msdb.dbo.sysjobs_view ov ORDER BY name", conn);
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

    private List<JobStep> GetAllStepsInternal()
    {
      var steps = new List<JobStep>();
      using (var conn = new SqlConnection(_connString))
      {
        conn.Open();  
        var cmd =
          new SqlCommand("SELECT s.job_id, s.step_id, s.step_uid, s.step_name, s.command FROM msdb.dbo.sysjobsteps AS s WHERE s.subsystem = 'TSQL'", conn);
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