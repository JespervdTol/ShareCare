using MySql.Data.MySqlClient;
using ShareCare.Module;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ShareCare.Services
{
    public class EventService
    {
        private readonly DatabaseService _databaseService;

        public EventService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Event>> GetEventsAsync()
        {
            var query = @"
                SELECT e.id AS EventId, e.summary AS EventSummary, e.date AS EventDate, 
                       e.startTime AS EventStartTime, e.endTime AS EventEndTime,
                       u.id AS UserId, u.firstname AS FirstName, u.lastname AS LastName,
                       r.id AS RoomId, r.name AS RoomName, r.building_id AS BuildingID
                FROM event e
                LEFT JOIN event_user eu ON e.id = eu.event_id
                LEFT JOIN user u ON eu.user_id = u.id
                LEFT JOIN event_room er ON e.id = er.event_id
                LEFT JOIN room r ON er.room_id = r.id
                WHERE e.date >= CURDATE()
                ORDER BY e.date, e.startTime";

            var dataTable = await _databaseService.ExecuteQueryAsync(query);

            var events = new Dictionary<int, Event>();

            foreach (DataRow row in dataTable.Rows)
            {
                var eventId = (int)row["EventId"];

                if (!events.ContainsKey(eventId))
                {
                    events[eventId] = new Event
                    {
                        EventID = eventId,
                        Summary = row["EventSummary"].ToString(),
                        Date = (DateTime)row["EventDate"],
                        StartTime = ConvertToTimeOnly(row["EventStartTime"]),
                        EndTime = ConvertToTimeOnly(row["EventEndTime"]),
                        Persons = new List<Person>(),
                        Rooms = new List<Room>() 
                    };
                }

                if (row["UserId"] != DBNull.Value)
                {
                    var person = new Person
                    {
                        PersonID = (int)row["UserId"],
                        FirstName = row["FirstName"].ToString(),
                        LastName = row["LastName"].ToString()
                    };

                    if (events[eventId].Persons.All(p => p.PersonID != person.PersonID))
                    {
                        events[eventId].Persons.Add(person);
                    }
                }

                if (row["RoomId"] != DBNull.Value)
                {
                    var room = new Room
                    {
                        RoomID = (int)row["RoomId"],
                        Name = row["RoomName"].ToString(),
                        BuildingID = (int)row["BuildingID"]
                    };

                    if (events[eventId].Rooms.All(r => r.RoomID != room.RoomID))
                    {
                        events[eventId].Rooms.Add(room);
                    }
                }
            }

            return new List<Event>(events.Values);
        }

        public async Task<bool> DeleteEventAsync(int eventId)
        {
            var deleteEventUserQuery = "DELETE FROM event_user WHERE event_id = @EventId";
            var deleteEventQuery = "DELETE FROM event WHERE id = @EventId";

            var parameters = new MySqlParameter[] { new MySqlParameter("@EventId", eventId) };

            try
            {
                await _databaseService.ExecuteNonQueryAsync(deleteEventUserQuery, parameters);

                var rowsAffected = await _databaseService.ExecuteNonQueryAsync(deleteEventQuery, parameters);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting event: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddEventAsync(string summary, DateTime date, TimeOnly startTime, TimeOnly endTime, List<int> personIds, List<int> roomIds)
        {
            try
            {
                var addEventQuery = @"
                    INSERT INTO event (summary, date, startTime, endTime) 
                    VALUES (@Summary, @Date, @StartTime, @EndTime);
                    SELECT LAST_INSERT_ID();";

                var eventParameters = new MySqlParameter[]
                {
                    new MySqlParameter("@Summary", summary),
                    new MySqlParameter("@Date", date),
                    new MySqlParameter("@StartTime", startTime),
                    new MySqlParameter("@EndTime", endTime)
                };

                var eventId = await _databaseService.ExecuteScalarAsync(addEventQuery, eventParameters);

                if (eventId == null)
                    return false;

                if (personIds != null && personIds.Count > 0)
                {
                    var addEventUserQuery = @"
                        INSERT INTO event_user (event_id, user_id) 
                        VALUES (@EventId, @UserId)";

                    foreach (var personId in personIds)
                    {
                        var eventUserParameters = new MySqlParameter[]
                        {
                            new MySqlParameter("@EventId", eventId),
                            new MySqlParameter("@UserId", personId)
                        };

                        await _databaseService.ExecuteNonQueryAsync(addEventUserQuery, eventUserParameters);
                    }
                }

                if (roomIds != null && roomIds.Count > 0)
                {
                    var addEventRoomQuery = @"
                        INSERT INTO event_room (event_id, room_id) 
                        VALUES (@EventId, @RoomId)";

                    foreach (var roomId in roomIds)
                    {
                        var eventRoomParameters = new MySqlParameter[]
                        {
                            new MySqlParameter("@EventId", eventId),
                            new MySqlParameter("@RoomId", roomId)
                        };

                        await _databaseService.ExecuteNonQueryAsync(addEventRoomQuery, eventRoomParameters);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding event: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Room>> GetRoomsAsync()
        {
            var query = "SELECT id AS RoomID, name AS RoomName, building_id AS BuildingID FROM room ORDER BY name";
            var dataTable = await _databaseService.ExecuteQueryAsync(query);

            var rooms = new List<Room>();

            foreach (DataRow row in dataTable.Rows)
            {
                rooms.Add(new Room
                {
                    RoomID = (int)row["RoomID"],
                    Name = row["RoomName"].ToString(),
                    BuildingID = (int)row["BuildingID"]
                });
            }

            return rooms;
        }

        public async Task<List<Event>> GetEventsForWeekAsync(DateTime startDate)
        {
            var query = @"
        SELECT e.id AS EventId, e.summary AS EventSummary, e.date AS EventDate, 
               e.startTime AS EventStartTime, e.endTime AS EventEndTime,
               u.id AS UserId, u.firstname AS FirstName, u.lastname AS LastName,
               r.id AS RoomId, r.name AS RoomName, r.building_id AS BuildingID
        FROM event e
        LEFT JOIN event_user eu ON e.id = eu.event_id
        LEFT JOIN user u ON eu.user_id = u.id
        LEFT JOIN event_room er ON e.id = er.event_id
        LEFT JOIN room r ON er.room_id = r.id
        WHERE e.date BETWEEN @StartDate AND @EndDate
        ORDER BY e.date, e.startTime";

            var parameters = new MySqlParameter[]
            {
        new MySqlParameter("@StartDate", startDate),
        new MySqlParameter("@EndDate", startDate.AddDays(6))
            };

            var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

            var events = new Dictionary<int, Event>();

            foreach (DataRow row in dataTable.Rows)
            {
                var eventId = (int)row["EventId"];

                if (!events.ContainsKey(eventId))
                {
                    events[eventId] = new Event
                    {
                        EventID = eventId,
                        Summary = row["EventSummary"].ToString(),
                        Date = (DateTime)row["EventDate"],
                        StartTime = ConvertToTimeOnly(row["EventStartTime"]),
                        EndTime = ConvertToTimeOnly(row["EventEndTime"]),
                        Persons = new List<Person>(),
                        Rooms = new List<Room>()
                    };
                }

                if (row["UserId"] != DBNull.Value)
                {
                    var person = new Person
                    {
                        PersonID = (int)row["UserId"],
                        FirstName = row["FirstName"].ToString(),
                        LastName = row["LastName"].ToString()
                    };

                    if (events[eventId].Persons.All(p => p.PersonID != person.PersonID))
                    {
                        events[eventId].Persons.Add(person);
                    }
                }

                if (row["RoomId"] != DBNull.Value)
                {
                    var room = new Room
                    {
                        RoomID = (int)row["RoomId"],
                        Name = row["RoomName"].ToString(),
                        BuildingID = (int)row["BuildingID"]
                    };

                    if (events[eventId].Rooms.All(r => r.RoomID != room.RoomID))
                    {
                        events[eventId].Rooms.Add(room);
                    }
                }
            }

            return new List<Event>(events.Values);
        }

        private TimeOnly ConvertToTimeOnly(object dbTime)
        {
            if (dbTime == DBNull.Value || dbTime == null)
                return default;

            TimeSpan timeSpan = (TimeSpan)dbTime;
            return new TimeOnly(timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
    }
}
