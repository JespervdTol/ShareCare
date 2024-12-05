using MySql.Data.MySqlClient;
using System;
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

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var query = "DELETE FROM task WHERE id = @TaskId";
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@TaskId", taskId)
            };

            var rowsAffected = await _databaseService.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> AddTaskAsync(int TaskTypeID, string summary, DateTime date, int userId)
        {
            var query = @"
                INSERT INTO task (type_id, summary, date, user_id) 
                VALUES (@TaskTypeID, @Summary, @Date, @UserId)";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@TaskTypeID", TaskTypeID),
                new MySqlParameter("@Summary", summary),
                new MySqlParameter("@Date", date),
                new MySqlParameter("@UserId", userId)
            };

            var rowsAffected = await _databaseService.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }


        public async Task<List<TaskType>> GetTaskTypesAsync()
        {
            var query = "SELECT id, name FROM task_type";

            var parameters = new List<MySqlParameter>();

            var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters.ToArray());

            var taskTypes = new List<TaskType>();

            foreach (DataRow row in dataTable.Rows)
            {
                taskTypes.Add(new TaskType
                {
                    TaskTypeID = (int)row["id"],
                    Name = row["name"].ToString()
                });
            }

            return taskTypes;
        }

    }
}