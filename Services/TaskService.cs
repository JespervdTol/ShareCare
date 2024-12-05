using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ShareCare.Module;

namespace ShareCare.Services
{
    public class TaskService
    {
        private readonly DatabaseService _databaseService;

        public TaskService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<ShareCare.Module.Task>> GetOpenTasksAsync()
        {
            var query = @"
                SELECT t.id AS TaskId, tt.name AS TaskType, t.summary AS TaskSummary, t.date AS TaskDate, 
                       CONCAT(u.firstname, ' ', u.lastname) AS Person
                FROM task t
                JOIN task_type tt ON t.type_id = tt.id
                JOIN user u ON t.user_id = u.id
                WHERE t.date >= CURDATE()";

            var parameters = new List<MySqlParameter>();

            var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters.ToArray());

            var tasks = new List<ShareCare.Module.Task>();

            foreach (DataRow row in dataTable.Rows)
            {
                tasks.Add(new ShareCare.Module.Task
                {
                    TaskID = (int)row["TaskId"],
                    Type = row["TaskType"].ToString(),
                    Summary = row["TaskSummary"].ToString(),
                    Date = (DateTime)row["TaskDate"],
                    Person = row["Person"].ToString()
                });
            }

            return tasks;
        }
    }
}