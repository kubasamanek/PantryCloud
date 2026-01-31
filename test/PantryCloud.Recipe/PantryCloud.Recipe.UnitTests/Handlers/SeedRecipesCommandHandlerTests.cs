using ErrorOr;
using NSubstitute;
using PantryCloud.Recipe.Application.Commands;
using PantryCloud.Recipe.Application.Commands.Handlers;
using PantryCloud.Recipe.Application.Dtos;
using PantryCloud.Recipe.Application.Interfaces;
using Shouldly;

namespace PantryCloud.Recipe.UnitTests.Handlers;

public class SeedRecipesCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnRecipesCreated_WhenRepositorySucceeds()
    {
        var repo = Substitute.For<IRecipeRepository>();
        repo.SeedRecipesAsync(Arg.Any<List<Core.Entities.Recipe>>(), Arg.Any<CancellationToken>())
            .Returns(new SeedRecipesResponseDto(3));

        var logger = TestHelper.MockLogger<SeedRecipesCommandHandler>();
        var handler = new SeedRecipesCommandHandler(repo, logger);
        var command = new SeedRecipesCommand(new SeedRecipesRequestDto(3));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.RecipesCreated.ShouldBe(3);
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectCount_ToRepository()
    {
        var repo = Substitute.For<IRecipeRepository>();
        repo.SeedRecipesAsync(Arg.Any<List<Core.Entities.Recipe>>(), Arg.Any<CancellationToken>())
            .Returns(call => new SeedRecipesResponseDto(call.ArgAt<List<Core.Entities.Recipe>>(0).Count));

        var logger = TestHelper.MockLogger<SeedRecipesCommandHandler>();
        var handler = new SeedRecipesCommandHandler(repo, logger);
        var command = new SeedRecipesCommand(new SeedRecipesRequestDto(5));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.RecipesCreated.ShouldBe(5);
        await repo.Received(1).SeedRecipesAsync(
            Arg.Is<List<Core.Entities.Recipe>>(r => r.Count == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenRepositoryFails()
    {
        var repo = Substitute.For<IRecipeRepository>();
        repo.SeedRecipesAsync(Arg.Any<List<Core.Entities.Recipe>>(), Arg.Any<CancellationToken>())
            .Returns(ErrorOr<SeedRecipesResponseDto>.From([Error.Failure("Seed.Failed", "Database error")]));

        var logger = TestHelper.MockLogger<SeedRecipesCommandHandler>();
        var handler = new SeedRecipesCommandHandler(repo, logger);
        var command = new SeedRecipesCommand(new SeedRecipesRequestDto(3));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "Seed.Failed");
    }
}
