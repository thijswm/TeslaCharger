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
    }
}
