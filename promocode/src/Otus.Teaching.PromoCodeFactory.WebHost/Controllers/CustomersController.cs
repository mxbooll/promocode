using Microsoft.AspNetCore.Mvc;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Otus.Teaching.PromoCodeFactory.WebHost.Controllers
{
    /// <summary>
    /// Клиенты
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Preference> _preferenceRepository;
        private readonly IRepository<CustomerPreference> _customerPreference;
        private readonly IRepository<PromoCode> _promoCodeRepository;

        public CustomersController(IRepository<Customer> customerRepository,
                                   IRepository<Preference> preferenceRepository,
                                   IRepository<CustomerPreference> customerPreference,
                                   IRepository<PromoCode> promoCodeRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _preferenceRepository = preferenceRepository ?? throw new ArgumentNullException(nameof(preferenceRepository));
            _customerPreference = customerPreference ?? throw new ArgumentNullException(nameof(customerPreference));
            _promoCodeRepository = promoCodeRepository ?? throw new ArgumentNullException(nameof(promoCodeRepository));
        }

        /// <summary>
        /// Получить данные всех клиентов
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<CustomerShortResponse>> GetCustomersAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            var preferences = await _preferenceRepository.GetAllAsync();

            var response = customers.Select(x => new CustomerShortResponse()
            {
                Id = x.Id,
                Email = x.Email,
                FirstName = x.FirstName,
                LastName = x.LastName
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Получить данные клиента по id
        /// </summary>
        /// <param name="id"> Идентификатор клиента </param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CustomerResponse>> GetCustomerAsync(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer is null)
            {
                return NotFound();
            }
            var customerPreferences = await _customerPreference.GetWhere(x => x.CustomerId == customer.Id);
            var preferences = await _preferenceRepository.GetWhere(x => customerPreferences.Select(s => s.PreferenceId).Contains(x.Id));
            var promocodes = await _promoCodeRepository.GetWhere(x => x.CustomerId == customer.Id);

            var response = new CustomerResponse
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Preferences = preferences
                    .Select(s => new PreferenceResponse
                    {
                        Id = s.Id,
                        Name = s.Name
                    }).ToList(),
                PromoCodes = promocodes.Select(s => new PromoCodeShortResponse
                {
                    Id = s.Id,
                    BeginDate = s.BeginDate.ToString(),
                    Code = s.Code,
                    EndDate = s.EndDate.ToString(),
                    PartnerName = s.PartnerName,
                    ServiceInfo = s.ServiceInfo
                }).ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Создать клиента
        /// </summary>
        /// <param name="request"> Данные о клиенте </param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateCustomerAsync(CreateOrEditCustomerRequest request)
        {
            var preferences = new List<Preference>();
            foreach (var preferenceId in request.PreferenceIds)
            {
                var preference = await _preferenceRepository.GetByIdAsync(preferenceId);
                if (preference == null)
                    return BadRequest();
                preferences.Add(preference);
            }
           
            var customer = new Customer()
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
            };

            var customerPreferences = preferences.Select(x => new CustomerPreference()
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                Customer = customer,
                PreferenceId = x.Id,
                Preference = x
            }).ToList();

            customer.Preferences = customerPreferences;

            await _customerRepository.AddAsync(customer);

            return Ok();
        }

        /// <summary>
        /// Редактирование данных клиента по идентификатору
        /// </summary>
        /// <param name="id"> Идентификатор клиента </param>
        /// <param name="request"> Новые данные о клиенте </param>
        /// <returns></returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> EditCustomersAsync(Guid id, CreateOrEditCustomerRequest request)
        {
            var preferences = new List<Preference>();
            foreach (var preferenceId in request.PreferenceIds)
            {
                var preference = await _preferenceRepository.GetByIdAsync(preferenceId);
                if (preference == null)
                    return BadRequest();
                preferences.Add(preference);
            }

            var customer = await _customerRepository.GetByIdAsync(id);

            var customerPreference = await _customerPreference.GetWhere(x => x.CustomerId == id);
            foreach (var item in customerPreference)
            {
                await _customerPreference.DeleteAsync(item);
            }

            var customerPreferences = preferences.Select(x => new CustomerPreference()
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                Customer = customer,
                PreferenceId = x.Id,
                Preference = x
            }).ToList();

            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.Email = request.Email;
            customer.Preferences = customerPreferences;            


            await _customerRepository.UpdateAsync(customer);

            return NoContent();
        }

        /// <summary>
        /// Удаление клиента по идентификатору
        /// </summary>
        /// <param name="id"> Идентификатор клиента </param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);

            if (customer == null)
                return NotFound();

            await _customerRepository.DeleteAsync(customer);

            var promocodes = await _promoCodeRepository.GetWhere(x => x.CustomerId == id);
            foreach (var item in promocodes)
            {
                await _promoCodeRepository.DeleteAsync(item);
            }

            var customerPreference = await _customerPreference.GetWhere(x => x.CustomerId == id);
            foreach (var item in customerPreference)
            {
                await _customerPreference.DeleteAsync(item);
            }

            return NoContent();
        }
    }
}