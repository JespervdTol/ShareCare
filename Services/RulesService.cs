using MySql.Data.MySqlClient;
using ShareCare.Module;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ShareCare.Services
{
    public class RulesService
    {
        private readonly DatabaseService _databaseService;

        public RulesService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<ShareCare.Module.Rule>> GetRulesAsync()
        {
            var query = "SELECT id AS RuleId, description AS RuleDescription, last_updated AS LastUpdated FROM rules ORDER BY id";

            var dataTable = await _databaseService.ExecuteQueryAsync(query);

            var rules = new List<ShareCare.Module.Rule>();

            foreach (DataRow row in dataTable.Rows)
            {
                try
                {
                    var rule = new ShareCare.Module.Rule
                    {
                        RuleId = row["RuleId"] != DBNull.Value ? (int)row["RuleId"] : 0,
                        Description = row["RuleDescription"] != DBNull.Value ? row["RuleDescription"].ToString() : string.Empty,
                        LastUpdated = row["LastUpdated"] != DBNull.Value ? (DateTime?)row["LastUpdated"] : null
                    };

                    rules.Add(rule);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing row: {ex.Message}");
                    continue;
                }
            }

            return rules;
        }

        public async Task<bool> AddRuleAsync(ShareCare.Module.Rule rule)
        {
            try
            {
                var query = "INSERT INTO rules (description, last_updated) VALUES (@Description, @LastUpdated);";
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@Description", rule.Description),
                    new MySqlParameter("@LastUpdated", DateTime.Now)
                };

                var rowsAffected = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding rule: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteRuleAsync(int ruleId)
        {
            var query = "DELETE FROM rules WHERE id = @RuleId";

            var parameters = new MySqlParameter[] { new MySqlParameter("@RuleId", ruleId) };

            try
            {
                var rowsAffected = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting rule: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateRuleAsync(ShareCare.Module.Rule rule)
        {
            try
            {
                var query = "UPDATE rules SET description = @Description, last_updated = @LastUpdated WHERE id = @RuleId;";
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@Description", rule.Description),
                    new MySqlParameter("@LastUpdated", DateTime.Now),
                    new MySqlParameter("@RuleId", rule.RuleId)
                };

                var rowsAffected = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating rule: {ex.Message}");
                return false;
            }
        }
    }
}