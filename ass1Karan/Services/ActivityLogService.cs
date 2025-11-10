using System;
using System.Threading.Tasks;
using ass1Karan.Models;

namespace ass1Karan.Services
{
    public class ActivityLogService
    {
        private readonly StudentDBContext _context;

        public ActivityLogService(StudentDBContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string userId, string action)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(action))
                return;

            var log = new ActivityLog
            {
                UserId = userId,
                Action = action,
                Timestamp = DateTime.Now
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
