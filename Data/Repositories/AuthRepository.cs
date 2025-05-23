using Data.Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Data.Repositories;

public class AuthRepository(DataContext context) : BaseRepository<UserEntity>(context), IAuthRepository
{
    
}
