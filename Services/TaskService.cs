using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ShareCare.Module;
using Microsoft.AspNetCore.Components.Authorization;

namespace ShareCare.Services
{
    public class TaskService
    {
        private readonly DatabaseService _databaseService;
        private readonly UserService _userService;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;

        public TaskService(DatabaseService databaseService, UserService userService, CustomAuthenticationStateProvider authenticationStateProvider)
        {
            _databaseService = databaseService;
            _userService = userService;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<List<ShareCare.Module.Task>> GetOpenTasksAsync()
        {
            var loggedInUserPersonId = _authenticationStateProvider.GetCurrentUserPersonId();

            if (loggedInUserPersonId == 0)
            {
                return new List<ShareCare.Module.Task>();
            }

            var query = @"
                SELECT t.id AS TaskId, tt.name AS TaskType, t.summary AS TaskSummary, t.date AS TaskDate, 
                       GROUP_CONCAT(DISTINCT CONCAT(u.firstname, ' ', u.lastname) SEPARATOR ', ') AS Persons, r.name AS RoomName
                FROM task t
                JOIN task_type tt ON t.type_id = tt.id
                JOIN task_user tp ON t.id = tp.task_id
                JOIN user u ON tp.user_id = u.id
                LEFT JOIN room r ON t.room_id = r.id
                WHERE t.date >= CURDATE()
                AND t.id IN (
                    SELECT task_id
                    FROM task_user
                    WHERE user_id = @UserId
                )
                GROUP BY t.id, tt.name, t.summary, t.date, r.name";

            var parameters = new List<MySqlParameter>
    {
        new MySqlParameter("@UserId", loggedInUserPersonId)
    };

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
                    Persons = ParsePersons(row["Persons"].ToString()),
                    RoomName = row["RoomName"]?.ToString()
                });
            }

            return tasks;
        }

        private List<Person> ParsePersons(string personsString)
        {
            var persons = new List<Person>();

            if (string.IsNullOrEmpty(personsString))
            {
                return persons;
            }

            var personNames = personsString.Split(", ");
            foreach (var personName in personNames)
            {
                var names = personName.Split(' ');
                if (names.Length >= 2)
                {
                    persons.Add(new Person
                    {
                        FirstName = names[0],
                        LastName = names[1]
                    });
                }
            }

            return persons;
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

        public async Task<bool> AddTaskAsync(int taskTypeId, string summary, DateTime date, List<int> personIds, int roomId)
        {
            try
            {
                var addTaskQuery = @"
                    INSERT INTO task (type_id, summary, date, room_id) 
                    VALUES (@TaskTypeId, @Summary, @Date, @RoomId);
                    SELECT LAST_INSERT_ID();";
                var taskParameters = new MySqlParameter[]
                {
                    new MySqlParameter("@TaskTypeId", taskTypeId),
                    new MySqlParameter("@Summary", summary),
                    new MySqlParameter("@Date", date),
                    new MySqlParameter("@RoomId", roomId)
                };

                var taskId = await _databaseService.ExecuteScalarAsync(addTaskQuery, taskParameters);

                if (taskId == null)
                {
                    return false;
                }

                if (personIds != null && personIds.Count > 0)
                {
                    var addTaskPersonQuery = @"
                INSERT INTO task_user (task_id, user_id) 
                VALUES (@TaskId, @UserId)";

                    foreach (var personId in personIds)
                    {
                        var taskPersonParameters = new MySqlParameter[]
                        {
                    new MySqlParameter("@TaskId", taskId),
                    new MySqlParameter("@UserId", personId)
                        };

                        await _databaseService.ExecuteNonQueryAsync(addTaskPersonQuery, taskPersonParameters);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding task: {ex.Message}");
                return false;
            }
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
                       GROUP_CONCAT(CONCAT(u.firstname, ' ', u.lastname) SEPARATOR ', ') AS Persons, r.name AS RoomName
                FROM task t
                JOIN task_type tt ON t.type_id = tt.id
                JOIN task_user tp ON t.id = tp.task_id
                JOIN user u ON tp.user_id = u.id
                LEFT JOIN room r ON t.room_id = r.id
                WHERE t.date BETWEEN @StartDate AND @EndDate
                GROUP BY t.id, tt.name, t.summary, t.date, r.name";

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
                    Persons = ParsePersons(row["Persons"].ToString()),
                    RoomName = row["RoomName"]?.ToString()
                });
            }

            return tasks;
        }
    }
}