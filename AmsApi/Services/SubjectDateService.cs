using AmsApi.Data;
using AmsApi.DTOs;
using AmsApi.Interfaces;
using AmsApi.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AmsApi.Services;

public class SubjectDateService : ISubjectDateService
{
    private readonly AmsDbContext _context;
    private readonly IMapper _mapper;

    public SubjectDateService(AmsDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<SubjectDate>> GetDatesBySubjectId(int subjectId)
    {
        return await _context.SubjectDates
            .Where(s => s.SubjectId == subjectId)
            .OrderBy(d => d.Date)
            .ToListAsync();
    }

    public async Task<SubjectDate> AddAsync(CreateSubjectDateDto dto)
    {
        var subjectDate = _mapper.Map<SubjectDate>(dto);
        _context.SubjectDates.Add(subjectDate);
        await _context.SaveChangesAsync();
        return subjectDate;
    }
}
