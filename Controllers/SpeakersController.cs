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
    public class SpeakersController : ControllerBase
    {
        public ICampRepository repository { get; }
        public IMapper mapper { get; }
        public LinkGenerator linkGenerator { get; }

        public SpeakersController(ICampRepository _repository, IMapper _mapper, LinkGenerator _linkGenerator)
        {
            repository = _repository;
            mapper = _mapper;
            linkGenerator = _linkGenerator;
        }

        public async Task<ActionResult<SpeakerModel[]>> GetSpeakers()
        {
            try
            {
                var talks = await repository.GetAllSpeakersAsync();
                return mapper.Map<SpeakerModel[]>(talks);
            }
            catch (Exception error)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SpeakerModel>> GetSpeaker(int id)
        {
            try
            {
                var speaker = await repository.GetSpeakerAsync(id);
                if (speaker == null) return NotFound($"Could not find speaker with id: {id}");

                return mapper.Map<SpeakerModel>(speaker);
            }
            catch (Exception error)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<SpeakerTargetModel>> CreateSpeaker(SpeakerTargetModel model)
        {
            try
            {
                var speaker = mapper.Map<Speaker>(model);
                repository.Add(speaker);

                if (await repository.SaveChangesAsync())
                {
                    var url = linkGenerator.GetPathByAction("GetSpeaker", "Speakers", values: new { id = speaker.SpeakerId });

                    return Created(url, mapper.Map<SpeakerTargetModel>(speaker));
                }
            }
            catch (Exception error)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }

            return BadRequest("Failed to save new Talk");
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult<SpeakerTargetModel>> UpdateSpeaker(int id, SpeakerTargetModel model)
        {
            try
            {
                var speakerToUpdate = await repository.GetSpeakerAsync(id);
                if (speakerToUpdate == null) return NotFound($"Could not find speaker with id: {id}");

                mapper.Map(model, speakerToUpdate);

                if (await repository.SaveChangesAsync())
                {
                    return mapper.Map<SpeakerTargetModel>(speakerToUpdate);
                }
            }
            catch (Exception error)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }

            return BadRequest();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSpeaker(string moniker, int id)
        {
            try
            {
                var speakerToDelete = await repository.GetSpeakerAsync(id);
                if (speakerToDelete == null) return NotFound($"Could not find speaker with id: {id}");

                repository.Delete(speakerToDelete);
                if (await repository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception error)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }

            return BadRequest("Failed to delete speaker");
        }
    }
}
