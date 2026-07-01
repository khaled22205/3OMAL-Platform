using API.Controllers.V1;
using Application.Features.Categories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _categoryServiceMock = new();

    private CategoriesController CreateController()
    {
        var controller = new CategoriesController(_categoryServiceMock.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    [Fact]
    public async Task GetAll_Should_return_200_with_categories()
    {
        _categoryServiceMock.Setup(x => x.GetTreeAsync())
            .ReturnsAsync(new List<CategoryTreeResponse> { new() { Id = 1, Name = "Plumbing" } });
        var controller = CreateController();

        var result = await controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Should_return_200_when_found()
    {
        _categoryServiceMock.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new CategoryResponse { Id = 1, Name = "Plumbing" });
        var controller = CreateController();

        var result = await controller.GetById(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_Should_return_404_when_not_found()
    {
        _categoryServiceMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((CategoryResponse?)null);
        var controller = CreateController();

        var result = await controller.GetById(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_Should_return_201()
    {
        _categoryServiceMock.Setup(x => x.CreateAsync(It.IsAny<CategoryRequest>()))
            .ReturnsAsync(new CategoryResponse { Id = 1, Name = "New Category" });
        var controller = CreateController();

        var result = await controller.Create(new CategoryRequest());

        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task Update_Should_return_200()
    {
        _categoryServiceMock.Setup(x => x.UpdateAsync(1, It.IsAny<CategoryRequest>()))
            .ReturnsAsync(new CategoryResponse { Id = 1, Name = "Updated" });
        var controller = CreateController();

        var result = await controller.Update(1, new CategoryRequest());

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_Should_return_200_when_successful()
    {
        _categoryServiceMock.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.Delete(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_Should_return_404_when_not_found()
    {
        _categoryServiceMock.Setup(x => x.DeleteAsync(999)).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.Delete(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ToggleActive_Should_return_200_when_successful()
    {
        _categoryServiceMock.Setup(x => x.ToggleActiveAsync(1)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.ToggleActive(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ToggleActive_Should_return_404_when_not_found()
    {
        _categoryServiceMock.Setup(x => x.ToggleActiveAsync(999)).ReturnsAsync(false);
        var controller = CreateController();

        var result = await controller.ToggleActive(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
