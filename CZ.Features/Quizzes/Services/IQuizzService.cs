using api.CZ.Features.Quizzes.DTOs;

namespace api.CZ.Features.Quizzes.Services;

public interface IQuizzService
{
    Task<IEnumerable<GetQuizzDto>> GetAllAsync();
    Task<GetQuizzDetailDto?> GetByIdAsync(Guid id);
    Task<GetQuizzDetailDto?> CreateAsync(CreateQuizzDto dto, Guid adminId);
    Task<GetQuizzDto?> UpdateAsync(Guid id, UpdateQuizzDto dto, Guid adminId);
    Task<bool> DeleteAsync(Guid id, Guid adminId);
}
