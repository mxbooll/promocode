using Microsoft.AspNetCore.Mvc;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Otus.Teaching.PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Предпочтения
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PreferenceController : ControllerBase
    {
        private readonly IRepository<Preference> _preferenceRepository;

        public PreferenceController(IRepository<Preference> preferenceRepository)
        {
            _preferenceRepository = preferenceRepository ?? throw new System.ArgumentNullException(nameof(preferenceRepository));
        }

        /// <summary>
        /// Получить список предпочтений
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<PreferenceResponse>> GetCustomersAsync()
        {
            var preferences = await _preferenceRepository.GetAllAsync();

            var response = preferences.Select(s => new PreferenceResponse()
            {
                Id = s.Id,
                Name = s.Name
            }).ToList();

            return Ok(response);
        }
    }
}
