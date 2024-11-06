using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentTeacherManagement.Core.Interfaces;
using StudentTeacherManagement.Core.Models;
using StudentTeacherManagement.DTOs;

namespace StudentTeacherManagement.Controllers;

[ApiController]
[Route("groups")]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IMapper _mapper;

    public GroupController(IGroupService groupService, IMapper mapper)
    {
        _groupService = groupService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupDTO>>> GetGroups([FromQuery] string? name = null,
                                                                  [FromQuery] int skip = 0,
                                                                  [FromQuery] int take = 10)
    {
        var groups = await _groupService.GetGroups(name, skip, take);
        return Ok(_mapper.Map<IEnumerable<GroupDTO>>(groups));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GroupDTO>> GetGroupById(Guid id, CancellationToken cancellationToken = default)
    {
        var group = await _groupService.GetGroupById(id, cancellationToken);
        if (group == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<GroupDTO>(group));
    }

    [HttpPost]
    public async Task<ActionResult<GroupDTO>> AddGroup(GroupDTO groupDto, CancellationToken cancellationToken = default)
    {
        var group = _mapper.Map<Group>(groupDto);
        var addedGroup = await _groupService.AddGroup(group, cancellationToken);
        return CreatedAtAction(nameof(GetGroupById), new { id = addedGroup.Id }, _mapper.Map<GroupDTO>(addedGroup));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroup(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _groupService.DeleteGroup(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{groupId}/students/{studentId}")]
    public async Task<IActionResult> AddStudentToGroup(Guid groupId, Guid studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _groupService.AddStudentToGroup(groupId, studentId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}