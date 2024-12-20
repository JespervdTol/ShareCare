using MySql.Data.MySqlClient;
using ShareCare.Module;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ShareCare.Services
{
    public class PaymentService
    {
        private readonly DatabaseService _databaseService;

        public PaymentService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<Payment>> GetPaymentsAsync()
        {
            var query = @"
                SELECT p.id AS PaymentId, p.amount AS PaymentAmount, p.summary AS PaymentSummary,
                       p.peopleAmount AS PaymentPeopleAmount, p.link AS PaymentLink, 
                       p.task_id AS TaskID
                FROM payment p
                ORDER BY p.id";

            var dataTable = await _databaseService.ExecuteQueryAsync(query);

            var payments = new List<Payment>();

            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    var payment = new Payment
                    {
                        PaymentID = row["PaymentId"] != DBNull.Value ? (int)row["PaymentId"] : 0,
                        Amount = row["PaymentAmount"] != DBNull.Value ? Convert.ToDecimal(row["PaymentAmount"]) : 0.0m,
                        Summary = row["PaymentSummary"] != DBNull.Value ? row["PaymentSummary"].ToString() : string.Empty,
                        PeopleAmount = row["PaymentPeopleAmount"] != DBNull.Value ? Convert.ToInt32(row["PaymentPeopleAmount"]) : 0,
                        Link = row["PaymentLink"] != DBNull.Value ? row["PaymentLink"].ToString() : string.Empty,
                        TaskID = row["TaskID"] != DBNull.Value ? Convert.ToInt32(row["TaskID"]) : 0
                    };

                    payments.Add(payment);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing row: {ex.Message}");
                    continue;
                }
            }

            return payments;
        }

        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            var deletePaymentUserQuery = "DELETE FROM user_payment WHERE payment_id = @PaymentId";
            var deletePaymentQuery = "DELETE FROM payment WHERE id = @PaymentId";

            var parameters = new MySqlParameter[] { new MySqlParameter("@PaymentId", paymentId) };

            try
            {
                await _databaseService.ExecuteNonQueryAsync(deletePaymentUserQuery, parameters);

                var rowsAffected = await _databaseService.ExecuteNonQueryAsync(deletePaymentQuery, parameters);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting payment: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddPaymentAsync(Payment payment)
        {
            try
            {
                var checkTaskExistsQuery = "SELECT COUNT(*) FROM task WHERE id = @TaskID";
                var taskExistsParameters = new MySqlParameter[] { new MySqlParameter("@TaskID", payment.TaskID) };
                var taskExists = await _databaseService.ExecuteScalarAsync(checkTaskExistsQuery, taskExistsParameters);

                if (taskExists == DBNull.Value || Convert.ToInt32(taskExists) == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Task ID does not exist.");
                    return false;
                }

                var addPaymentQuery = @"
                    INSERT INTO payment (amount, summary, peopleAmount, link, task_id) 
                    VALUES (@Amount, @Summary, @PeopleAmount, @Link, @TaskID);
                    SELECT LAST_INSERT_ID();";

                var paymentParameters = new MySqlParameter[]
                {
                    new MySqlParameter("@Amount", payment.Amount),
                    new MySqlParameter("@Summary", payment.Summary),
                    new MySqlParameter("@PeopleAmount", payment.PeopleAmount),
                    new MySqlParameter("@Link", payment.Link),
                    new MySqlParameter("@TaskID", payment.TaskID)
                };

                var paymentId = await _databaseService.ExecuteScalarAsync(addPaymentQuery, paymentParameters);

                if (paymentId == null)
                    return false;

                if (payment.UserIDs != null && payment.UserIDs.Count > 0)
                {
                    var addPaymentUserQuery = @"
                        INSERT INTO user_payment (payment_id, user_id) 
                        VALUES (@PaymentID, @UserID)";

                    foreach (var userId in payment.UserIDs)
                    {
                        var userPaymentParameters = new MySqlParameter[]
                        {
                            new MySqlParameter("@PaymentID", paymentId),
                            new MySqlParameter("@UserID", userId)
                        };

                        await _databaseService.ExecuteNonQueryAsync(addPaymentUserQuery, userPaymentParameters);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding payment: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Person>> GetUsersForPaymentAsync(int paymentId)
        {
            var query = @"
                SELECT u.id AS UserId, u.firstname AS FirstName, u.lastname AS LastName
                FROM user_payment up
                JOIN user u ON up.user_id = u.id
                WHERE up.payment_id = @PaymentId";

            var parameters = new MySqlParameter[] { new MySqlParameter("@PaymentId", paymentId) };

            var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

            var users = new List<Person>();

            foreach (DataRow row in dataTable.Rows)
            {
                var person = new Person
                {
                    PersonID = row["UserId"] != DBNull.Value ? (int)row["UserId"] : 0,
                    FirstName = row["FirstName"] != DBNull.Value ? row["FirstName"].ToString() : string.Empty,
                    LastName = row["LastName"] != DBNull.Value ? row["LastName"].ToString() : string.Empty
                };

                users.Add(person);
            }

            return users;
        }

        public async Task<ShareCare.Module.Task> GetTaskDetailsAsync(int taskId)
        {
            var query = @"
                SELECT t.id AS TaskID, t.type AS TaskType, t.summary AS TaskSummary
                FROM task t
                WHERE t.id = @TaskID";

            var parameters = new MySqlParameter[] { new MySqlParameter("@TaskID", taskId) };

            var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            var row = dataTable.Rows[0];

            return new ShareCare.Module.Task
            {
                TaskID = row["TaskID"] != DBNull.Value ? (int)row["TaskID"] : 0,
                Type = row["TaskType"] != DBNull.Value ? row["TaskType"].ToString() : string.Empty,
                Summary = row["TaskSummary"] != DBNull.Value ? row["TaskSummary"].ToString() : string.Empty
            };
        }
    }
}