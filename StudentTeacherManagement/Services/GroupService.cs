using Microsoft.EntityFrameworkCore;
using StudentTeacherManagement.Core.Interfaces;
using StudentTeacherManagement.Core.Models;
using StudentTeaherManagement.Storage;

namespace StudentTeacherManagement.Services;

public class GroupService : IGroupService
{
    private readonly DataContext _context;

    public GroupService(DataContext context)
    {
        _context = context;
    }

    #region DQL

    public async Task<IEnumerable<Group>> GetGroups(string? name, int skip, int take, CancellationToken cancellationToken = default)
    {
        var groups = _context.Groups.AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            groups = groups.Where(g => g.Name.Contains(name));
        }

        return await groups.OrderBy(g => g.Name)
            .Skip(skip)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Group?> GetGroupById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    #endregion

    #region DML

    public async Task<Group> AddGroup(Group group, CancellationToken cancellationToken = default)
    {
        await _context.Groups.AddAsync(group, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return group;
    }

    public async Task DeleteGroup(Guid id, CancellationToken cancellationToken = default)
    {
        var group = await _context.Groups.FindAsync(new object[] { id }, cancellationToken);
        if (group == null)
        {
            throw new KeyNotFoundException($"Group with id {id} not found.");
        }

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddStudentToGroup(Guid groupId, Guid studentId, CancellationToken cancellationToken = default)
    {
        var group = await _context.Groups.Include(g => g.Students).FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);
        if (group == null)
        {
            throw new KeyNotFoundException($"Group with id {groupId} not found.");
        }

        var student = await _context.Students.FindAsync(new object[] { studentId }, cancellationToken);
        if (student == null)
        {
            throw new KeyNotFoundException($"Student with id {studentId} not found.");
        }

        group.Students.Add(student);
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}