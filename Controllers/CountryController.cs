using FlightSearch.DTOs.InModels;
using FlightSearch.DTOs.OutModels;
using FlightSearch.Interfaces;
using FlightSearch.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace FlightSearch.Controllers
{
    [Route("api/country")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly ICountryRepository _countryRepository;

        public CountryController(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCountries()
        {
            var countries = await _countryRepository.GetAllCountries();
            var countriesDto = countries.Select(country => country.ToOutCountryDTO());
            return Ok(countriesDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCountryById([FromRoute] int id)
        {
            var country = await _countryRepository.GetCountryById(id);
            if (country == null)
            {
                return NotFound();
            }
            return Ok(country.ToOutCountryDTO());
        }

        [HttpPost("{countryName}")]
        public async Task<IActionResult> CreateCountry([FromRoute] string countryName)
        {
            var country = await _countryRepository.CreateCountry(countryName);
            return CreatedAtAction(nameof(GetCountryById), new { id = country.Id }, country.ToOutCountryDTO());
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCountry([FromBody] OutCountryDTO countryDTO)
        {
            var country = await _countryRepository.UpdateCountry(countryDTO.Id, countryDTO.CountryName);
            if (country == null)
            {
                return NotFound();
            } 
            return Ok(country.ToOutCountryDTO());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry([FromRoute] int id)
        {
            var country = await _countryRepository.DeleteCountry(id);
            if (country == null)
            {
                return NotFound();
            }
            return NoContent();
        }
        
    }
}