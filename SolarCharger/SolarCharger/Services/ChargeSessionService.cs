using Microsoft.EntityFrameworkCore;
using SolarCharger.Controllers.ViewModels;
using SolarCharger.EF;

namespace SolarCharger.Services
{
    public class ChargeSessionService(ChargeContext context)
    {
        public async Task<IEnumerable<ChargeSessionViewModel>> GetChargeSessionsAsync()
        {
            var chargeSessions = await context.ChargeSessions
                .AsNoTracking()
                .Select(a => ChargeSessionViewModel.FromChargeSession(a))
                .ToListAsync();
            return chargeSessions;
        }

        public async Task AddChargeSessionAsync(ChargeSession chargeSession)
        {
            await context.ChargeSessions.AddAsync(chargeSession);
            await context.SaveChangesAsync();
        }

        public async Task UpdateChargeSessionAsync(ChargeSession chargeSession)
        {
            context.ChargeSessions.Update(chargeSession);
            await context.SaveChangesAsync();
        }

        public async Task AddCurrentChangeAsync(ChargeCurrentChange currentChange)
        {
            await context.ChargeCurrentChanges.AddAsync(currentChange);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ChargeCurrentChangeViewModel>> GetCurrentChangesAsync()
        {
            var currentChanges = await context.ChargeCurrentChanges
                .AsNoTracking()
                .Select(a => ChargeCurrentChangeViewModel.FromChargeCurrentChange(a))
                .ToListAsync();
            return currentChanges;
        }

        public async Task AddPowerHistoryAsync(ChargePower chargePower)
        {
            await context.ChargePowers.AddAsync(chargePower);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PowerHistory>> GetPowerHistoryAsync(Guid sessionId)
        {
            var powerHistory = await context.ChargePowers
                .AsNoTracking()
                .Where(a => a.ChargeSessionId == sessionId)
                .Select(a => new PowerHistory
                {
                    Time = a.Timestamp,
                    Power = a.Power,
                    CompensatedPower = a.CompensatedPower
                }
                )
                .ToListAsync();
            return powerHistory;
        }

        public async Task<IEnumerable<ChargeCurrentChangeViewModel>> GetCurrentChangesAsync(Guid sessionId)
        {
            var currentChanges = await context.ChargeCurrentChanges
                .AsNoTracking()
                .Where(a => a.ChargeSessionId == sessionId)
                .Select(a => ChargeCurrentChangeViewModel.FromChargeCurrentChange(a))
                .ToListAsync();
            return currentChanges;
        }

        public async Task UpdateRefreshTokenAsync(string refreshToken)
        {
            var settings = await context.Settings.FirstOrDefaultAsync();
            if (settings != null)
            {
                settings.TeslaRefreshToken = refreshToken;
                context.Entry(settings).Property(a => a.TeslaRefreshToken).IsModified = true;
                await context.SaveChangesAsync();
            }
        }
    }
}
