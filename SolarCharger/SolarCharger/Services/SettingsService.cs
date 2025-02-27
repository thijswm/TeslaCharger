using Microsoft.EntityFrameworkCore;
using SolarCharger.Controllers.ViewModels;
using SolarCharger.EF;

namespace SolarCharger.Services
{
    public class SettingsService
    {
        private readonly ChargeContext _context;

        public SettingsService(ChargeContext context)
        {
            _context = context;
        }

        public async Task<SettingsViewModel?> GetSettingsAsync()
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();
            return settings != null ? SettingsViewModel.FromModel(settings) : null;
        }

        public async Task UpdateSettingsAsync(Settings settings)
        {
            var allSettings = await _context.Settings.ToListAsync();
            _context.Settings.RemoveRange(allSettings);
            await _context.Settings.AddAsync(settings);
            await _context.SaveChangesAsync();
        }
    }
}
