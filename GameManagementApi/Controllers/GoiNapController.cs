using GameManagement.Application.DTOs;
using GameManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoiNapController : ControllerBase
    {
        private readonly IGoiNapService _goiNapService;

        public GoiNapController(IGoiNapService goiNapService)
        {
            _goiNapService = goiNapService;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll() 
        {
            var goiNap = await _goiNapService.GetAllAsync();

            return Ok(goiNap);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        { 
            var goiNap = await _goiNapService.GetByIdAsync(id);

            if(goiNap == null)
                return NotFound();

            return Ok(goiNap);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(CreateGoiNapDto dto)
        {
            var goiNap = await _goiNapService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = goiNap.GoiNapId },
                goiNap
            );
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(int id, UpdateGoiNapDto dto)
        {
            var goiNap =  await _goiNapService.UpdateAsync(id, dto);

            if (goiNap == null)
                return NotFound();
            
            return Ok(goiNap);
        }
    }
}
