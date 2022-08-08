using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampsController : ControllerBase
    {
        public ICampRepository repository { get; }
        public IMapper mapper { get; }
        public LinkGenerator linkGenerator { get; }

        public CampsController(ICampRepository _repository, IMapper _mapper, LinkGenerator _linkGenerator)
        {
            repository = _repository;
            mapper = _mapper;
            linkGenerator = _linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> GetCamps(bool includeTalks = false)
        {
            try
            {
                var results = await repository.GetAllCampsAsync(includeTalks);
                //CampModel[] campModel = mapper.Map<CampModel[]>(results);
                return mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> GetCampsBySearchDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await repository.GetAllCampsByEventDate(theDate, includeTalks);

                if (!results.Any()) return NotFound();

                return mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> GetCamp(string moniker)
        {
            try
            {
                var result = await repository.GetCampAsync(moniker);
                if (result == null) { 
                    return NotFound(); 
                }

                //CampModel[] campModel = mapper.Map<CampModel[]>(result);

                return mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CampModel>> CreateCamp(CampModel model)
        {
            try
            {

                var findCamp = await repository.GetCampAsync(model.Moniker);
                if (findCamp != null)
                {
                    return BadRequest("This moniker is already in used");
                }

                var location = linkGenerator.GetPathByAction("GetCamp", "Camps", new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                var camp = mapper.Map<Camp>(model);

                repository.Add(camp);
                
                if (await repository.SaveChangesAsync())
                {
                    return Created($"/api/camps/{camp.Moniker}", mapper.Map<CampModel>(camp));
                }
            }
            catch (Exception error)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }

            return BadRequest();
        }
    
        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> UpdateCamp(string moniker, CampModel model)
        {
             try
            {
                var campToUpdate = await repository.GetCampAsync(moniker);
                if (campToUpdate == null) return NotFound($"Could not find camp with moniker: {moniker}");

                mapper.Map(model, campToUpdate);
                if (await repository.SaveChangesAsync())
                {
                    return mapper.Map<CampModel>(campToUpdate);
                }
            } 
            catch (Exception error)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }

            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> DeleteCamp(string moniker)
        {
            try
            {
                var campToDelete = await repository.GetCampAsync(moniker);
                if (campToDelete == null) return NotFound($"Could not find camp with moniker: {moniker}");

                repository.Delete(campToDelete);
                if (await repository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception error)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }

            return BadRequest();
        }
    }
}
