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
    [Route("api/camps/{moniker}/[controller]")]
    public class TalksController : ControllerBase
    {
        public ICampRepository repository { get; }
        public IMapper mapper { get; }
        public LinkGenerator linkGenerator { get; }

        public TalksController(ICampRepository _repository, IMapper _mapper, LinkGenerator _linkGenerator)
        {
            repository = _repository;
            mapper = _mapper;
            linkGenerator = _linkGenerator;
        }

        public async Task<ActionResult<TalkModel[]>> GetTalks(string moniker, bool includeSpeakers = false)
        {
            try
            {
                var talks = await repository.GetTalksByMonikerAsync(moniker, includeSpeakers);
                return mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception error)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> GetTalk(string moniker, int id, bool includeSpeakers = false)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, id, includeSpeakers);
                if (talk == null) return NotFound($"Moniker {moniker} has no talk with id: {id}");
                return mapper.Map<TalkModel>(talk);
            }
            catch (Exception error)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> CreateTalk(string moniker, TalkModel model)
        {
            try
            {
                var camp = await repository.GetCampAsync(moniker);
                if (camp == null) return BadRequest("Camp does not exist");

                var talk = mapper.Map<Talk>(model);
                talk.Camp = camp;

                if (model.Speaker == null) return BadRequest("Speaker ID is required");
                var speaker = await repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker could not be found with this SpeakerId");
                talk.Speaker = speaker;

                repository.Add(talk);

                if (await repository.SaveChangesAsync())
                {
                    var url = linkGenerator.GetPathByAction(
                            HttpContext,
                            "GetTalk", 
                            values: new { moniker, id = talk.TalkId, includeSpeakers = true }
                        );

                    return Created(url, mapper.Map<TalkModel>(talk));
                }
            }
            catch (Exception error)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }

            return BadRequest("Failed to save new Talk");
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> UpdateTalk(string moniker, int id, TalkModel model)
        {
            try
            {
                var talkToUpdate = await repository.GetTalkByMonikerAsync(moniker, id, true);
                if (talkToUpdate == null) return NotFound($"Could not find talk with id: {id}");
                
                mapper.Map(model, talkToUpdate);

                if (model.Speaker != null)
                {
                    var speaker = await repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null) talkToUpdate.Speaker = speaker;
                }

                if (await repository.SaveChangesAsync())
                {
                    return mapper.Map<TalkModel>(talkToUpdate);
                }
            }
            catch (Exception error)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }

            return BadRequest();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTalk(string moniker, int id)
        {
            try
            {
                var talkToDelete = await repository.GetTalkByMonikerAsync(moniker, id);
                if (talkToDelete == null) return NotFound($"Could not find talk with id: {id}");

                repository.Delete(talkToDelete);
                if (await repository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception error)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, error.Message);
            }

            return BadRequest("Failed to delete talk");
        }
    }
}
