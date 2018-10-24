using System.Linq;
using System.Threading.Tasks;
using Kinoheld.Domain.Model.Model;
using Kinoheld.Domain.Services.Abstractions.Database;
using Microsoft.EntityFrameworkCore;

namespace Kinoheld.Domain.Services.Database
{
    public class KinoheldDbAccess : IKinoheldDbAccess
    {
        private readonly KinoheldDbContext m_context;

        public KinoheldDbAccess(KinoheldDbContext context)
        {
            m_context = context;
        }

        public async Task SaveCityCinemaPreferenceAsync(string userId, string city, string cinemaJson)
        {
            using (var transaction = await m_context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                var existantUser = 
                    await m_context.User.Include(p => p.CityCinemaAssignments)
                    .FirstOrDefaultAsync(p => p.AlexaId == userId).ConfigureAwait(false);
                if (existantUser == null)
                {
                    await AddNewUser(userId, city, cinemaJson).ConfigureAwait(false);
                    await m_context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return;
                }

                var existantAssignment = existantUser.CityCinemaAssignments
                    .FirstOrDefault(p => p.City == city);
                if (existantAssignment == null)
                {
                    AddNewAssignment(existantUser, city, cinemaJson);

                    await m_context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return;
                }

                existantAssignment.Cinema = cinemaJson;
                await m_context.SaveChangesAsync().ConfigureAwait(false);
                transaction.Commit();
            }
        }

        public async Task<KinoheldUser> GetPreferenceAsync(string userId)
        {
            var user = 
                await m_context.User.Include(p => p.CityCinemaAssignments)
                    .FirstOrDefaultAsync(p => p.AlexaId == userId).ConfigureAwait(false);

            return user;
        }

        public async Task TestConnectionAsync()
        {
            await m_context.User.FindAsync((long)0).ConfigureAwait(false);
        }

        public async Task SetEmailPreferenceAsync(string userId, bool disableEmails)
        {
            using (var transaction = await m_context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                var existantUser =
                    await m_context.User
                        .FirstOrDefaultAsync(p => p.AlexaId == userId).ConfigureAwait(false);
                if (existantUser == null)
                {
                    await AddNewUser(userId, disableEmails: disableEmails).ConfigureAwait(false);
                    await m_context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return;
                }

                existantUser.DisableEmails = disableEmails;
                await m_context.SaveChangesAsync().ConfigureAwait(false);
                transaction.Commit();
            }
        }

        public async Task SaveCityPreferenceAsync(string userId, string city)
        {
            using (var transaction = await m_context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                var existantUser =
                    await m_context.User
                        .FirstOrDefaultAsync(p => p.AlexaId == userId).ConfigureAwait(false);
                if (existantUser == null)
                {
                    await AddNewUser(userId, city).ConfigureAwait(false);
                    await m_context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return;
                }

                existantUser.City = city;
                await m_context.SaveChangesAsync().ConfigureAwait(false);
                transaction.Commit();
            }
        }

        public async Task DeleteUserPreferenceAsync(string userId)
        {
            using (var transaction = await m_context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                var existantUser = await m_context.User.FirstOrDefaultAsync(p => p.AlexaId == userId).ConfigureAwait(false);
                if (existantUser != null)
                {
                    m_context.User.Remove(existantUser);
                    await m_context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }
            }
        }

        private static void AddNewAssignment(KinoheldUser existantUser, string city, string cinemaJson)
        {
            existantUser.CityCinemaAssignments.Add(new CityCinemaAssignment
            {
                City = city,
                Cinema = cinemaJson,
                User = existantUser
            });
        }

        private async Task AddNewUser(string userId, string city = null, string cinemaJson = null, bool disableEmails = false)
        {
            var newUser = await m_context.User.AddAsync(new KinoheldUser
            {
                AlexaId = userId,
                City = city,
                DisableEmails = disableEmails
            }).ConfigureAwait(false);

            if (string.IsNullOrEmpty(city) ||
                string.IsNullOrEmpty(cinemaJson))
            {
                return;
            }

            newUser.Entity.CityCinemaAssignments.Add(new CityCinemaAssignment
            {
                City = city,
                Cinema = cinemaJson,
                User = newUser.Entity
            });
        }
    }
}