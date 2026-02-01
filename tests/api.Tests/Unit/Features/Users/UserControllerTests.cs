using System.Security.Claims;
using api.CZ.Features.Users;
using api.CZ.Features.Users.Services;
using api.CZ.Features.Users.UserDtos;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests.Unit.Features.Users;

public class UserControllerTests
{
    private readonly Mock<IUserService> _mockService;
    private readonly Mock<ILogger<UserController>> _mockLogger;
    private readonly UserController _controller;
    private readonly Guid _testUserId;

    public UserControllerTests()
    {
        _mockService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UserController>>();
        _controller = new UserController(_mockService.Object, _mockLogger.Object);
        _testUserId = Guid.NewGuid();

        // Setup user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region GetProfile Tests

    [Fact]
    public async Task GetProfile_WhenUserExists_ShouldReturnOkWithProfile()
    {
        // Arrange
        var expectedProfile = new GetUserProfileDto
        {
            Id = _testUserId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            MemberSince = DateTime.UtcNow.AddDays(-30),
            ThumbnailUrl = "https://example.com/avatar.jpg"
        };

        _mockService
            .Setup(s => s.GetProfileAsync(_testUserId))
            .ReturnsAsync(expectedProfile);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedProfile);

        _mockService.Verify(s => s.GetProfileAsync(_testUserId), Times.Once);
    }

    [Fact]
    public async Task GetProfile_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        _mockService
            .Setup(s => s.GetProfileAsync(_testUserId))
            .ReturnsAsync((GetUserProfileDto?)null);

        // Act
        var result = await _controller.GetProfile();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockService.Verify(s => s.GetProfileAsync(_testUserId), Times.Once);
    }

    #endregion

    #region UpdateProfile Tests

    [Fact]
    public async Task UpdateProfile_WhenSuccessful_ShouldReturnOkWithUpdatedProfile()
    {
        // Arrange
        var updateDto = new UpdateUserProfileDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            ThumbnailUrl = "https://example.com/new-avatar.jpg"
        };

        var expectedProfile = new GetUserProfileDto
        {
            Id = _testUserId,
            Email = "test@example.com",
            FirstName = updateDto.FirstName,
            LastName = updateDto.LastName,
            MemberSince = DateTime.UtcNow.AddDays(-30),
            ThumbnailUrl = updateDto.ThumbnailUrl
        };

        _mockService
            .Setup(s => s.UpdateProfileAsync(_testUserId, updateDto))
            .ReturnsAsync(expectedProfile);

        // Act
        var result = await _controller.UpdateProfile(updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedProfile);

        _mockService.Verify(s => s.UpdateProfileAsync(_testUserId, updateDto), Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var updateDto = new UpdateUserProfileDto
        {
            FirstName = "Jane",
            LastName = "Smith"
        };

        _mockService
            .Setup(s => s.UpdateProfileAsync(_testUserId, updateDto))
            .ReturnsAsync((GetUserProfileDto?)null);

        // Act
        var result = await _controller.UpdateProfile(updateDto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockService.Verify(s => s.UpdateProfileAsync(_testUserId, updateDto), Times.Once);
    }

    #endregion

    #region DeleteAccount Tests

    [Fact]
    public async Task DeleteAccount_WhenSuccessful_ShouldReturnOkWithMessage()
    {
        // Arrange
        _mockService
            .Setup(s => s.DeleteAccountAsync(_testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAccount();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;

        _mockService.Verify(s => s.DeleteAccountAsync(_testUserId), Times.Once);
    }

    [Fact]
    public async Task DeleteAccount_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        _mockService
            .Setup(s => s.DeleteAccountAsync(_testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteAccount();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockService.Verify(s => s.DeleteAccountAsync(_testUserId), Times.Once);
    }

    #endregion
}
