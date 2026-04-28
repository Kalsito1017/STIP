using SofiaTransport.Domain.Entities;

namespace SofiaTransport.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
