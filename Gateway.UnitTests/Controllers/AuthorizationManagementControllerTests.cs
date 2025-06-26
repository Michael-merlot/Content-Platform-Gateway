using Gateway.Api.Controllers.Auth;
using Gateway.Api.Mappers;
using Gateway.Api.Models.Auth;
using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using NSubstitute;

using Shouldly;

using Endpoint = Gateway.Core.Models.Auth.Endpoint;

namespace Gateway.UnitTests.Controllers;

public sealed class AuthorizationManagementControllerTests
{
    private readonly IAuthorizationManagementService _authService = Substitute.For<IAuthorizationManagementService>();
    private readonly IEnumerable<EndpointDataSource> _endpointSources = Substitute.For<IEnumerable<EndpointDataSource>>();
    private readonly AuthorizationManagementController _controller;

    public AuthorizationManagementControllerTests() =>
        _controller = new AuthorizationManagementController(_authService, _endpointSources)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

    [Fact]
    public async Task GetRole_ServiceSuccess_ReturnsRole()
    {
        const long roleId = 1;
        Role role = new()
        {
            Id = roleId,
            Name = "TestRole",
            IsAdmin = false
        };

        _authService.GetRoleAsync(roleId).Returns(role);

        IActionResult result = await _controller.GetRole(roleId);

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(role.ToAdminDto());
    }

    [Fact]
    public async Task GetRole_ServiceError_ReturnsProblem()
    {
        const long roleId = 123;

        _authService.GetRoleAsync(roleId).Returns(AuthorizationManagementError.RoleNotFound);

        IActionResult result = await _controller.GetRole(roleId);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        problem.Value.ShouldBeOfType<ProblemDetails>();
    }

    [Fact]
    public async Task GetRoles_ServiceSuccess_ReturnsRoles()
    {
        Role[] roles =
        [
            new()
            {
                Id = 1,
                Name = "Admin",
                IsAdmin = true
            },
            new()
            {
                Id = 2,
                Name = "User",
                IsAdmin = false
            }
        ];

        _authService.GetRolesAsync().Returns(roles);

        IActionResult result = await _controller.GetRoles();

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        RoleCollectionAdminResponse response = ok.Value.ShouldBeOfType<RoleCollectionAdminResponse>();
        response.Roles.ToArray().ShouldBe(roles.ToAdminDto().Roles.ToArray());
    }

    [Fact]
    public async Task GetRoles_ServiceError_ReturnsProblem()
    {
        _authService.GetRolesAsync().Returns(AuthorizationManagementError.Unknown);

        IActionResult result = await _controller.GetRoles();

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task CreateRole_ServiceSuccess_ReturnsCreatedRole()
    {
        CreateRoleRequest request = new("TestRole");
        Role createdRole = new()
        {
            Id = 1,
            Name = "TestRole",
            IsAdmin = false
        };

        _authService.CreateRoleAsync(request.Name).Returns(createdRole);

        IActionResult result = await _controller.CreateRole(request);

        CreatedAtActionResult created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(_controller.GetRole));
        created.RouteValues.ShouldNotBeNull().ShouldContainKeyAndValue("roleId", createdRole.Id);
        created.Value.ShouldBe(createdRole);
    }

    [Fact]
    public async Task CreateRole_ServiceError_ReturnsProblem()
    {
        CreateRoleRequest request = new("ExistingRole");

        _authService.CreateRoleAsync(request.Name).Returns(AuthorizationManagementError.EntityAlreadyExists);

        IActionResult result = await _controller.CreateRole(request);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task DeleteRole_ServiceSuccess_ReturnsNoContent()
    {
        const long roleId = 1;

        _authService.DeleteRoleAsync(roleId).Returns(Result<AuthorizationManagementError>.Success());

        IActionResult result = await _controller.DeleteRole(roleId);

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteRole_ServiceError_ReturnsProblem()
    {
        const long roleId = 123;

        _authService.DeleteRoleAsync(roleId).Returns(AuthorizationManagementError.RoleNotFound);

        IActionResult result = await _controller.DeleteRole(roleId);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetRolePermissions_ServiceSuccess_ReturnsPermissions()
    {
        const long roleId = 1;
        Permission[] permissions =
        [
            new(1, "read.users", "Read users"),
            new(2, "write.users", "Write users")
        ];

        _authService.GetRolePermissionsAsync(roleId).Returns(permissions);

        IActionResult result = await _controller.GetRolePermissions(roleId);

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        PermissionCollectionResponse response = ok.Value.ShouldBeOfType<PermissionCollectionResponse>();
        response.Permissions.ToArray().ShouldBe(permissions.ToDto().Permissions.ToArray());
    }

    [Fact]
    public async Task GetRolePermissions_ServiceError_ReturnsProblem()
    {
        const long roleId = 123;

        _authService.GetRolePermissionsAsync(roleId).Returns(AuthorizationManagementError.RoleNotFound);

        IActionResult result = await _controller.GetRolePermissions(roleId);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task AddPermissionToRole_ServiceSuccess_ReturnsNoContent()
    {
        const long roleId = 1;
        AddPermissionToRoleRequest request = new(2);

        _authService.AddPermissionToRoleAsync(roleId, request.PermissionId)
            .Returns(Result<AuthorizationManagementError>.Success());

        IActionResult result = await _controller.AddPermissionToRole(roleId, request);

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task AddPermissionToRole_ServiceError_ReturnsProblem()
    {
        const long roleId = 123;
        AddPermissionToRoleRequest request = new(2);

        _authService.AddPermissionToRoleAsync(roleId, request.PermissionId)
            .Returns(AuthorizationManagementError.AnyEntityNotFound);

        IActionResult result = await _controller.AddPermissionToRole(roleId, request);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task RemovePermissionFromRole_ServiceSuccess_ReturnsNoContent()
    {
        const long roleId = 1;
        const long permissionId = 2;

        _authService.RemovePermissionFromRoleAsync(roleId, permissionId)
            .Returns(Result<AuthorizationManagementError>.Success());

        IActionResult result = await _controller.RemovePermissionFromRole(roleId, permissionId);

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RemovePermissionFromRole_ServiceError_ReturnsProblem()
    {
        const long roleId = 123;
        const long permissionId = 2;

        _authService.RemovePermissionFromRoleAsync(roleId, permissionId)
            .Returns(AuthorizationManagementError.AnyEntityNotFound);

        IActionResult result = await _controller.RemovePermissionFromRole(roleId, permissionId);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetUserRoles_ServiceSuccess_ReturnsRoles()
    {
        const int userId = 1;
        Role[] roles =
        [
            new()
            {
                Id = 1,
                Name = "Admin",
                IsAdmin = true
            },
            new()
            {
                Id = 2,
                Name = "User",
                IsAdmin = false
            }
        ];

        _authService.GetUserRolesAsync(userId).Returns(roles);

        IActionResult result = await _controller.GetUserRoles(userId);

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        RoleCollectionAdminResponse response = ok.Value.ShouldBeOfType<RoleCollectionAdminResponse>();
        response.Roles.ToArray().ShouldBe(roles.ToAdminDto().Roles.ToArray());
    }

    [Fact]
    public async Task GetUserRoles_ServiceError_ReturnsProblem()
    {
        const int userId = 123;

        _authService.GetUserRolesAsync(userId).Returns(AuthorizationManagementError.UserNotFound);

        IActionResult result = await _controller.GetUserRoles(userId);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task AddRoleToUser_ServiceSuccess_ReturnsNoContent()
    {
        const int userId = 1;
        AddRoleToUserRequest request = new(2);

        _authService.AddRoleToUserAsync(userId, request.RoleId)
            .Returns(Result<AuthorizationManagementError>.Success());

        IActionResult result = await _controller.AddRoleToUser(userId, request);

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task AddRoleToUser_ServiceError_ReturnsProblem()
    {
        const int userId = 123;
        AddRoleToUserRequest request = new(2);

        _authService.AddRoleToUserAsync(userId, request.RoleId)
            .Returns(AuthorizationManagementError.AnyEntityNotFound);

        IActionResult result = await _controller.AddRoleToUser(userId, request);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task RemoveRoleFromUser_ServiceSuccess_ReturnsNoContent()
    {
        const int userId = 1;
        const long roleId = 2;

        _authService.RemoveRoleFromUserAsync(userId, roleId)
            .Returns(Result<AuthorizationManagementError>.Success());

        IActionResult result = await _controller.RemoveRoleFromUser(userId, roleId);

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RemoveRoleFromUser_ServiceError_ReturnsProblem()
    {
        const int userId = 123;
        const long roleId = 2;

        _authService.RemoveRoleFromUserAsync(userId, roleId)
            .Returns(AuthorizationManagementError.AnyEntityNotFound);

        IActionResult result = await _controller.RemoveRoleFromUser(userId, roleId);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetPermission_ServiceSuccess_ReturnsPermission()
    {
        const long permissionId = 1;
        Permission permission = new(permissionId, "read.users", "Read users");

        _authService.GetPermissionAsync(permissionId).Returns(permission);

        IActionResult result = await _controller.GetPermission(permissionId);

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(permission.ToDto());
    }

    [Fact]
    public async Task GetPermission_ServiceError_ReturnsProblem()
    {
        const long permissionId = 123;

        _authService.GetPermissionAsync(permissionId).Returns(AuthorizationManagementError.PermissionNotFound);

        IActionResult result = await _controller.GetPermission(permissionId);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetPermissions_ServiceSuccess_ReturnsPermissions()
    {
        Permission[] permissions =
        [
            new(1, "read.users", "Read users"),
            new(2, "write.users", "Write users")
        ];

        _authService.GetPermissionsAsync().Returns(permissions);

        IActionResult result = await _controller.GetPermissions();

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        PermissionCollectionResponse response = ok.Value.ShouldBeOfType<PermissionCollectionResponse>();
        response.Permissions.ToArray().ShouldBe(permissions.ToDto().Permissions.ToArray());
    }

    [Fact]
    public async Task GetPermissions_ServiceError_ReturnsProblem()
    {
        _authService.GetPermissionsAsync().Returns(AuthorizationManagementError.Unknown);

        IActionResult result = await _controller.GetPermissions();

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task CreatePermission_ServiceSuccess_ReturnsCreatedPermission()
    {
        CreatePermissionRequest request = new("test.permission", "Test permission");
        Permission createdPermission = new(1, "test.permission", "Test permission");

        _authService.CreatePermissionAsync(request.Name, request.Description).Returns(createdPermission);

        IActionResult result = await _controller.CreatePermission(request);

        CreatedAtActionResult created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(_controller.GetPermission));
        created.RouteValues.ShouldNotBeNull().ShouldContainKeyAndValue("permissionId", createdPermission.Id);
        created.Value.ShouldBe(createdPermission);
    }

    [Fact]
    public async Task CreatePermission_ServiceError_ReturnsProblem()
    {
        CreatePermissionRequest request = new("existing.permission", "Existing permission");

        _authService.CreatePermissionAsync(request.Name, request.Description)
            .Returns(AuthorizationManagementError.EntityAlreadyExists);

        IActionResult result = await _controller.CreatePermission(request);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task DeletePermission_ServiceSuccess_ReturnsNoContent()
    {
        const long permissionId = 1;

        _authService.DeletePermissionAsync(permissionId).Returns(Result<AuthorizationManagementError>.Success());

        IActionResult result = await _controller.DeletePermission(permissionId);

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeletePermission_ServiceError_ReturnsProblem()
    {
        const long permissionId = 123;

        _authService.DeletePermissionAsync(permissionId).Returns(AuthorizationManagementError.PermissionNotFound);

        IActionResult result = await _controller.DeletePermission(permissionId);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetEndpoint_ServiceSuccess_ReturnsEndpoint()
    {
        const long endpointId = 1;
        Endpoint endpoint = new()
        {
            Id = endpointId,
            Controller = "Test",
            Action = "Get",
            HttpMethod = "GET"
        };

        _authService.GetEndpointAsync(endpointId).Returns(endpoint);

        IActionResult result = await _controller.GetEndpoint(endpointId);

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe(endpoint.ToDto());
    }

    [Fact]
    public async Task GetEndpoint_ServiceError_ReturnsProblem()
    {
        const long endpointId = 123;

        _authService.GetEndpointAsync(endpointId).Returns(AuthorizationManagementError.EndpointNotFound);

        IActionResult result = await _controller.GetEndpoint(endpointId);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetEndpoints_ServiceSuccess_ReturnsEndpoints()
    {
        Endpoint[] endpoints =
        [
            new()
            {
                Id = 1,
                Controller = "Test",
                Action = "Get",
                HttpMethod = "GET"
            },
            new()
            {
                Id = 2,
                Controller = "Test",
                Action = "Post",
                HttpMethod = "POST"
            }
        ];

        _authService.GetEndpointsAsync().Returns(endpoints);

        IActionResult result = await _controller.GetEndpoints();

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        EndpointCollectionResponse response = ok.Value.ShouldBeOfType<EndpointCollectionResponse>();
        response.Endpoints.ToArray().ShouldBe(endpoints.ToDto().Endpoints.ToArray());
    }

    [Fact]
    public async Task GetEndpoints_ServiceError_ReturnsProblem()
    {
        _authService.GetEndpointsAsync().Returns(AuthorizationManagementError.Unknown);

        IActionResult result = await _controller.GetEndpoints();

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task CreateEndpoint_ServiceSuccess_ReturnsCreatedEndpoint()
    {
        CreateEndpointRequest request = new("Test", "Action", "POST");
        Endpoint createdEndpoint = new()
        {
            Id = 1,
            Controller = "Test",
            Action = "Action",
            HttpMethod = "POST"
        };

        _authService.CreateEndpointAsync(request.Controller, request.Action, request.HttpMethod)
            .Returns(createdEndpoint);

        IActionResult result = await _controller.CreateEndpoint(request);

        CreatedAtActionResult created = result.ShouldBeOfType<CreatedAtActionResult>();
        created.ActionName.ShouldBe(nameof(_controller.GetEndpoint));
        created.RouteValues.ShouldNotBeNull().ShouldContainKeyAndValue("endpointId", createdEndpoint.Id);
        created.Value.ShouldBe(createdEndpoint);
    }

    [Fact]
    public async Task CreateEndpoint_ServiceError_ReturnsProblem()
    {
        CreateEndpointRequest request = new("Test", "Action", "POST");

        _authService.CreateEndpointAsync(request.Controller, request.Action, request.HttpMethod)
            .Returns(AuthorizationManagementError.EntityAlreadyExists);

        IActionResult result = await _controller.CreateEndpoint(request);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task DeleteEndpoint_ServiceSuccess_ReturnsNoContent()
    {
        const long endpointId = 1;

        _authService.DeleteEndpointAsync(endpointId).Returns(Result<AuthorizationManagementError>.Success());

        IActionResult result = await _controller.DeleteEndpoint(endpointId);

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteEndpoint_ServiceError_ReturnsProblem()
    {
        const long endpointId = 123;

        _authService.DeleteEndpointAsync(endpointId).Returns(AuthorizationManagementError.EndpointNotFound);

        IActionResult result = await _controller.DeleteEndpoint(endpointId);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetEndpointPermissionRequirements_ServiceSuccess_ReturnsPermissions()
    {
        const long endpointId = 1;
        Permission[] permissions =
        [
            new(1, "read.test", "Read test"),
            new(2, "write.test", "Write test")
        ];

        _authService.GetEndpointPermissionRequirementsAsync(endpointId).Returns(permissions);

        IActionResult result = await _controller.GetEndpointPermissionRequirements(endpointId);

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        PermissionCollectionResponse response = ok.Value.ShouldBeOfType<PermissionCollectionResponse>();
        response.Permissions.ToArray().ShouldBe(permissions.ToDto().Permissions.ToArray());
    }

    [Fact]
    public async Task GetEndpointPermissionRequirements_ServiceError_ReturnsProblem()
    {
        const long endpointId = 123;

        _authService.GetEndpointPermissionRequirementsAsync(endpointId)
            .Returns(AuthorizationManagementError.EndpointNotFound);

        IActionResult result = await _controller.GetEndpointPermissionRequirements(endpointId);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task AddPermissionRequirementToEndpoint_ServiceSuccess_ReturnsNoContent()
    {
        const long endpointId = 1;
        AddPermissionRequirementToEndpointRequest request = new(2);

        _authService.AddPermissionRequirementToEndpointAsync(endpointId, request.PermissionId)
            .Returns(Result<AuthorizationManagementError>.Success());

        IActionResult result = await _controller.AddPermissionRequirementToEndpoint(endpointId, request);

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task AddPermissionRequirementToEndpoint_ServiceError_ReturnsProblem()
    {
        const long endpointId = 123;
        AddPermissionRequirementToEndpointRequest request = new(2);

        _authService.AddPermissionRequirementToEndpointAsync(endpointId, request.PermissionId)
            .Returns(AuthorizationManagementError.AnyEntityNotFound);

        IActionResult result = await _controller.AddPermissionRequirementToEndpoint(endpointId, request);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task RemovePermissionRequirementFromEndpoint_ServiceSuccess_ReturnsNoContent()
    {
        const long endpointId = 1;
        const long permissionId = 2;

        _authService.RemovePermissionRequirementFromEndpointAsync(endpointId, permissionId)
            .Returns(Result<AuthorizationManagementError>.Success());

        IActionResult result = await _controller.RemovePermissionRequirementFromEndpoint(endpointId, permissionId);

        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RemovePermissionRequirementFromEndpoint_ServiceError_ReturnsProblem()
    {
        const long endpointId = 123;
        const long permissionId = 2;

        _authService.RemovePermissionRequirementFromEndpointAsync(endpointId, permissionId)
            .Returns(AuthorizationManagementError.AnyEntityNotFound);

        IActionResult result = await _controller.RemovePermissionRequirementFromEndpoint(endpointId, permissionId);

        ObjectResult problem = result.ShouldBeOfType<ObjectResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void GetFrameworkEndpoints_ReturnsFrameworkEndpoints()
    {
        IActionResult result = _controller.GetFrameworkEndpoints();

        OkObjectResult ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldNotBeNull();
    }
}
