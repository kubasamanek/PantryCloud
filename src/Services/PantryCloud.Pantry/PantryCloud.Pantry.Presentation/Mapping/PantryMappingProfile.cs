using AutoMapper;
using PantryCloud.Pantry.Application.Commands;
using PantryCloud.Pantry.Application.Dtos;

namespace PantryCloud.Pantry.Presentation.Mapping;

public class PantryMappingProfile : Profile
{
    public PantryMappingProfile()
    {
        CreateMap<CreatePantryItemRequestDto, CreatePantryItemCommand>()
            .ConstructUsing(src => new CreatePantryItemCommand(src));
    }
}

