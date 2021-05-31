using AutoMapper;
using StudentAPI.Model;

public class AutomapperProfile : Profile
{
    public AutomapperProfile()
    {
        CreateMap<Student, StudentDTO>();
    }
}

