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
                       CONCAT(u.firstname, ' ', u.lastname) AS Person, r.name AS RoomName
                FROM task t
                JOIN task_type tt ON t.type_id = tt.id
                JOIN user u ON t.user_id = u.id
                LEFT JOIN room r ON t.room_id = r.id
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
                    Person = row["Person"].ToString(),
                    RoomName = row["RoomName"]?.ToString()
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

        public async Task<bool> AddTaskAsync(int taskTypeId, string summary, DateTime date, int userId, int roomId)
        {
            var query = @"
                INSERT INTO task (type_id, summary, date, user_id, room_id) 
                VALUES (@TaskTypeId, @Summary, @Date, @UserId, @RoomId)";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@TaskTypeId", taskTypeId),
                new MySqlParameter("@Summary", summary),
                new MySqlParameter("@Date", date),
                new MySqlParameter("@UserId", userId),
                new MySqlParameter("@RoomId", roomId)
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

        public async Task<List<Building>> GetBuildingsAsync()
        {
            var query = @"
                SELECT b.id AS BuildingID, b.address AS BuildingAdrress, 
                       r.id AS RoomID, r.name AS RoomName
                FROM building b
                LEFT JOIN room r ON b.id = r.building_id
                ORDER BY b.id, r.name";

            var parameters = new List<MySqlParameter>();

            var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters.ToArray());

            var buildings = new List<Building>();
            var buildingDictionary = new Dictionary<int, Building>();

            foreach (DataRow row in dataTable.Rows)
            {
                int buildingId = (int)row["BuildingID"];
                if (!buildingDictionary.TryGetValue(buildingId, out var building))
                {
                    building = new Building
                    {
                        BuildingID = buildingId,
                        Address = row["BuildingAdrress"].ToString(),
                        Rooms = new List<Room>()
                    };
                    buildingDictionary[buildingId] = building;
                    buildings.Add(building);
                }

                if (row["RoomID"] != DBNull.Value)
                {
                    building.Rooms.Add(new Room
                    {
                        RoomID = (int)row["RoomID"],
                        Name = row["RoomName"].ToString(),
                        BuildingID = buildingId
                    });
                }
            }

            return buildings;
        }

        public async Task<List<ShareCare.Module.Task>> GetTasksForWeekAsync(DateTime startDate)
        {
            var query = @"
                SELECT t.id AS TaskId, tt.name AS TaskType, t.summary AS TaskSummary, t.date AS TaskDate, 
                       CONCAT(u.firstname, ' ', u.lastname) AS Person, r.name AS RoomName
                FROM task t
                JOIN task_type tt ON t.type_id = tt.id
                JOIN user u ON t.user_id = u.id
                LEFT JOIN room r ON t.room_id = r.id
                WHERE t.date BETWEEN @StartDate AND @EndDate
                ORDER BY t.date";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@StartDate", startDate),
                new MySqlParameter("@EndDate", startDate.AddDays(6))
            };

            var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

            var tasks = new List<ShareCare.Module.Task>();

            foreach (DataRow row in dataTable.Rows)
            {
                tasks.Add(new ShareCare.Module.Task
                {
                    TaskID = (int)row["TaskId"],
                    Type = row["TaskType"].ToString(),
                    Summary = row["TaskSummary"].ToString(),
                    Date = (DateTime)row["TaskDate"],
                    Person = row["Person"].ToString(),
                    RoomName = row["RoomName"]?.ToString()
                });
            }

            return tasks;
        }
    }
}