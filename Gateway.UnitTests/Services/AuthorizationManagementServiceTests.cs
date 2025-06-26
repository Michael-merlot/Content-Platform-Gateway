using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Interfaces.Persistence;
using Gateway.Core.Models;
using Gateway.Core.Models.Auth;
using Gateway.Core.Services.Auth;

using NSubstitute;

using Shouldly;

using Endpoint = Gateway.Core.Models.Auth.Endpoint;

namespace Gateway.UnitTests.Services;

public sealed class AuthorizationManagementServiceTests
{
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IPermissionRepository _permissionRepository = Substitute.For<IPermissionRepository>();
    private readonly IEndpointRepository _endpointRepository = Substitute.For<IEndpointRepository>();
    private readonly IUserAuthorizationRepository _userAuthorizationRepository = Substitute.For<IUserAuthorizationRepository>();
    private readonly IDistributedCacheService _cache = Substitute.For<IDistributedCacheService>();
    private readonly AuthorizationManagementService _service;

    public AuthorizationManagementServiceTests() =>
        _service = new AuthorizationManagementService(_roleRepository, _permissionRepository, _endpointRepository,
            _userAuthorizationRepository, _cache);

    [Fact]
    public async Task GetRoleAsync_RepositorySuccess_ReturnsRole()
    {
        const long roleId = 1;
        Role expectedRole = new()
        {
            Id = roleId,
            Name = "TestRole",
            IsAdmin = false
        };

        _roleRepository.GetRoleAsync(roleId).Returns(expectedRole);

        Result<Role, AuthorizationManagementError> result = await _service.GetRoleAsync(roleId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedRole);
    }

    [Fact]
    public async Task GetRoleAsync_RepositoryError_ReturnsError()
    {
        const long roleId = 123;

        _roleRepository.GetRoleAsync(roleId).Returns(AuthorizationManagementError.RoleNotFound);

        Result<Role, AuthorizationManagementError> result = await _service.GetRoleAsync(roleId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.RoleNotFound);
    }

    [Fact]
    public async Task GetRolesAsync_RepositorySuccess_ReturnsRoles()
    {
        Role[] expectedRoles =
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

        _roleRepository.GetRolesAsync().Returns(expectedRoles);

        Result<IEnumerable<Role>, AuthorizationManagementError> result = await _service.GetRolesAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ToArray().ShouldBe(expectedRoles);
    }

    [Fact]
    public async Task GetRolesAsync_RepositoryError_ReturnsError()
    {
        _roleRepository.GetRolesAsync().Returns(AuthorizationManagementError.Unknown);

        Result<IEnumerable<Role>, AuthorizationManagementError> result = await _service.GetRolesAsync();

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.Unknown);
    }

    [Fact]
    public async Task CreateRoleAsync_ValidName_CallsRepository()
    {
        const string roleName = "TestRole";
        Role expectedRole = new()
        {
            Id = 1,
            Name = roleName,
            IsAdmin = false
        };

        _roleRepository.CreateRoleAsync(roleName).Returns(expectedRole);

        Result<Role, AuthorizationManagementError> result = await _service.CreateRoleAsync(roleName);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedRole);
        await _roleRepository.Received(1).CreateRoleAsync(roleName);
    }

    [Fact]
    public async Task CreateRoleAsync_NameTooLong_ReturnsBadRequest()
    {
        string longName = new('a', 101);

        Result<Role, AuthorizationManagementError> result = await _service.CreateRoleAsync(longName);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.BadRequest);
        await _roleRepository.DidNotReceive().CreateRoleAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task CreateRoleAsync_RepositoryError_ReturnsError()
    {
        const string roleName = "ExistingRole";

        _roleRepository.CreateRoleAsync(roleName).Returns(AuthorizationManagementError.EntityAlreadyExists);

        Result<Role, AuthorizationManagementError> result = await _service.CreateRoleAsync(roleName);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.EntityAlreadyExists);
    }

    [Fact]
    public async Task DeleteRoleAsync_RepositorySuccess_ReturnsSuccess()
    {
        const long roleId = 1;

        _roleRepository.DeleteRoleAsync(roleId).Returns(Result<AuthorizationManagementError>.Success());

        Result<AuthorizationManagementError> result = await _service.DeleteRoleAsync(roleId);

        result.IsSuccess.ShouldBeTrue();
        await _roleRepository.Received(1).DeleteRoleAsync(roleId);
    }

    [Fact]
    public async Task DeleteRoleAsync_RepositoryError_ReturnsError()
    {
        const long roleId = 123;

        _roleRepository.DeleteRoleAsync(roleId).Returns(AuthorizationManagementError.RoleNotFound);

        Result<AuthorizationManagementError> result = await _service.DeleteRoleAsync(roleId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.RoleNotFound);
    }

    [Fact]
    public async Task GetRolePermissionsAsync_RepositorySuccess_ReturnsPermissions()
    {
        const long roleId = 1;
        Permission[] expectedPermissions =
        [
            new(1, "read.users", "Read users"),
            new(2, "write.users", "Write users")
        ];

        _roleRepository.GetRolePermissionsAsync(roleId).Returns(expectedPermissions);

        Result<IEnumerable<Permission>, AuthorizationManagementError> result = await _service.GetRolePermissionsAsync(roleId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ToArray().ShouldBe(expectedPermissions);
    }

    [Fact]
    public async Task GetRolePermissionsAsync_RepositoryError_ReturnsError()
    {
        const long roleId = 123;

        _roleRepository.GetRolePermissionsAsync(roleId).Returns(AuthorizationManagementError.RoleNotFound);

        Result<IEnumerable<Permission>, AuthorizationManagementError> result = await _service.GetRolePermissionsAsync(roleId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.RoleNotFound);
    }

    [Fact]
    public async Task AddPermissionToRoleAsync_RepositorySuccess_ReturnsSuccess()
    {
        const long roleId = 1;
        const long permissionId = 2;

        _roleRepository.AddPermissionToRoleAsync(roleId, permissionId).Returns(Result<AuthorizationManagementError>.Success());

        Result<AuthorizationManagementError> result = await _service.AddPermissionToRoleAsync(roleId, permissionId);

        result.IsSuccess.ShouldBeTrue();
        await _roleRepository.Received(1).AddPermissionToRoleAsync(roleId, permissionId);
    }

    [Fact]
    public async Task AddPermissionToRoleAsync_RepositoryError_ReturnsError()
    {
        const long roleId = 123;
        const long permissionId = 2;

        _roleRepository.AddPermissionToRoleAsync(roleId, permissionId).Returns(AuthorizationManagementError.AnyEntityNotFound);

        Result<AuthorizationManagementError> result = await _service.AddPermissionToRoleAsync(roleId, permissionId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.AnyEntityNotFound);
    }

    [Fact]
    public async Task RemovePermissionFromRoleAsync_RepositorySuccess_ReturnsSuccess()
    {
        const long roleId = 1;
        const long permissionId = 2;

        _roleRepository.RemovePermissionFromRoleAsync(roleId, permissionId).Returns(Result<AuthorizationManagementError>.Success());

        Result<AuthorizationManagementError> result = await _service.RemovePermissionFromRoleAsync(roleId, permissionId);

        result.IsSuccess.ShouldBeTrue();
        await _roleRepository.Received(1).RemovePermissionFromRoleAsync(roleId, permissionId);
    }

    [Fact]
    public async Task RemovePermissionFromRoleAsync_RepositoryError_ReturnsError()
    {
        const long roleId = 123;
        const long permissionId = 2;

        _roleRepository.RemovePermissionFromRoleAsync(roleId, permissionId).Returns(AuthorizationManagementError.AnyEntityNotFound);

        Result<AuthorizationManagementError> result = await _service.RemovePermissionFromRoleAsync(roleId, permissionId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.AnyEntityNotFound);
    }

    [Fact]
    public async Task GetUserRolesAsync_RepositorySuccess_ReturnsRoles()
    {
        const int userId = 1;
        Role[] expectedRoles =
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

        _userAuthorizationRepository.GetUserRolesAsync(userId).Returns(expectedRoles);

        Result<IEnumerable<Role>, AuthorizationManagementError> result = await _service.GetUserRolesAsync(userId, false);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ToArray().ShouldBe(expectedRoles);
    }

    [Fact]
    public async Task GetUserRolesAsync_RepositoryError_ReturnsError()
    {
        const int userId = 123;

        _userAuthorizationRepository.GetUserRolesAsync(userId).Returns(AuthorizationManagementError.UserNotFound);

        Result<IEnumerable<Role>, AuthorizationManagementError> result = await _service.GetUserRolesAsync(userId, false);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.UserNotFound);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_RepositorySuccess_ReturnsPermissions()
    {
        const int userId = 1;
        Permission[] expectedPermissions =
        [
            new(1, "read.users", "Read users"),
            new(2, "write.users", "Write users")
        ];

        _userAuthorizationRepository.GetUserPermissionsAsync(userId).Returns(expectedPermissions);

        Result<IEnumerable<Permission>, AuthorizationManagementError> result = await _service.GetUserPermissionsAsync(userId, false);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ToArray().ShouldBe(expectedPermissions);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_RepositoryError_ReturnsError()
    {
        const int userId = 123;

        _userAuthorizationRepository.GetUserPermissionsAsync(userId).Returns(AuthorizationManagementError.UserNotFound);

        Result<IEnumerable<Permission>, AuthorizationManagementError> result = await _service.GetUserPermissionsAsync(userId, false);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.UserNotFound);
    }

    [Fact]
    public async Task AddRoleToUserAsync_RepositorySuccess_ReturnsSuccess()
    {
        const int userId = 1;
        const long roleId = 2;

        _userAuthorizationRepository.AddRoleToUserAsync(userId, roleId).Returns(Result<AuthorizationManagementError>.Success());

        Result<AuthorizationManagementError> result = await _service.AddRoleToUserAsync(userId, roleId);

        result.IsSuccess.ShouldBeTrue();
        await _userAuthorizationRepository.Received(1).AddRoleToUserAsync(userId, roleId);
    }

    [Fact]
    public async Task AddRoleToUserAsync_RepositoryError_ReturnsError()
    {
        const int userId = 123;
        const long roleId = 2;

        _userAuthorizationRepository.AddRoleToUserAsync(userId, roleId).Returns(AuthorizationManagementError.AnyEntityNotFound);

        Result<AuthorizationManagementError> result = await _service.AddRoleToUserAsync(userId, roleId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.AnyEntityNotFound);
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_RepositorySuccess_ReturnsSuccess()
    {
        const int userId = 1;
        const long roleId = 2;

        _userAuthorizationRepository.RemoveRoleFromUserAsync(userId, roleId).Returns(Result<AuthorizationManagementError>.Success());

        Result<AuthorizationManagementError> result = await _service.RemoveRoleFromUserAsync(userId, roleId);

        result.IsSuccess.ShouldBeTrue();
        await _userAuthorizationRepository.Received(1).RemoveRoleFromUserAsync(userId, roleId);
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_RepositoryError_ReturnsError()
    {
        const int userId = 123;
        const long roleId = 2;

        _userAuthorizationRepository.RemoveRoleFromUserAsync(userId, roleId).Returns(AuthorizationManagementError.AnyEntityNotFound);

        Result<AuthorizationManagementError> result = await _service.RemoveRoleFromUserAsync(userId, roleId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.AnyEntityNotFound);
    }

    [Fact]
    public async Task GetPermissionAsync_RepositorySuccess_ReturnsPermission()
    {
        const long permissionId = 1;
        Permission expectedPermission = new(permissionId, "read.users", "Read users");

        _permissionRepository.GetPermissionAsync(permissionId).Returns(expectedPermission);

        Result<Permission, AuthorizationManagementError> result = await _service.GetPermissionAsync(permissionId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedPermission);
    }

    [Fact]
    public async Task GetPermissionAsync_RepositoryError_ReturnsError()
    {
        const long permissionId = 123;

        _permissionRepository.GetPermissionAsync(permissionId).Returns(AuthorizationManagementError.PermissionNotFound);

        Result<Permission, AuthorizationManagementError> result = await _service.GetPermissionAsync(permissionId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.PermissionNotFound);
    }

    [Fact]
    public async Task GetPermissionsAsync_RepositorySuccess_ReturnsPermissions()
    {
        Permission[] expectedPermissions =
        [
            new(1, "read.users", "Read users"),
            new(2, "write.users", "Write users")
        ];

        _permissionRepository.GetPermissionsAsync().Returns(expectedPermissions);

        Result<IEnumerable<Permission>, AuthorizationManagementError> result = await _service.GetPermissionsAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ToArray().ShouldBe(expectedPermissions);
    }

    [Fact]
    public async Task GetPermissionsAsync_RepositoryError_ReturnsError()
    {
        _permissionRepository.GetPermissionsAsync().Returns(AuthorizationManagementError.Unknown);

        Result<IEnumerable<Permission>, AuthorizationManagementError> result = await _service.GetPermissionsAsync();

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.Unknown);
    }

    [Fact]
    public async Task CreatePermissionAsync_ValidParameters_CallsRepository()
    {
        const string name = "test.permission";
        const string description = "Test permission";
        Permission expectedPermission = new(1, name, description);

        _permissionRepository.CreatePermissionAsync(name, description).Returns(expectedPermission);

        Result<Permission, AuthorizationManagementError> result = await _service.CreatePermissionAsync(name, description);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedPermission);
        await _permissionRepository.Received(1).CreatePermissionAsync(name, description);
    }

    [Fact]
    public async Task CreatePermissionAsync_NameTooLong_ReturnsBadRequest()
    {
        string longName = new('a', 101);
        const string description = "Test permission";

        Result<Permission, AuthorizationManagementError> result = await _service.CreatePermissionAsync(longName, description);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.BadRequest);
        await _permissionRepository.DidNotReceive().CreatePermissionAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task CreatePermissionAsync_DescriptionTooLong_ReturnsBadRequest()
    {
        const string name = "test.permission";
        string longDescription = new('a', 501);

        Result<Permission, AuthorizationManagementError> result = await _service.CreatePermissionAsync(name, longDescription);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.BadRequest);
        await _permissionRepository.DidNotReceive().CreatePermissionAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task CreatePermissionAsync_RepositoryError_ReturnsError()
    {
        const string name = "existing.permission";
        const string description = "Existing permission";

        _permissionRepository.CreatePermissionAsync(name, description).Returns(AuthorizationManagementError.EntityAlreadyExists);

        Result<Permission, AuthorizationManagementError> result = await _service.CreatePermissionAsync(name, description);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.EntityAlreadyExists);
    }

    [Fact]
    public async Task DeletePermissionAsync_RepositorySuccess_ReturnsSuccess()
    {
        const long permissionId = 1;

        _permissionRepository.DeletePermissionAsync(permissionId).Returns(Result<AuthorizationManagementError>.Success());

        Result<AuthorizationManagementError> result = await _service.DeletePermissionAsync(permissionId);

        result.IsSuccess.ShouldBeTrue();
        await _permissionRepository.Received(1).DeletePermissionAsync(permissionId);
    }

    [Fact]
    public async Task DeletePermissionAsync_RepositoryError_ReturnsError()
    {
        const long permissionId = 123;

        _permissionRepository.DeletePermissionAsync(permissionId).Returns(AuthorizationManagementError.PermissionNotFound);

        Result<AuthorizationManagementError> result = await _service.DeletePermissionAsync(permissionId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.PermissionNotFound);
    }

    [Fact]
    public async Task GetEndpointAsync_RepositorySuccess_ReturnsEndpoint()
    {
        const long endpointId = 1;
        Endpoint expectedEndpoint = new()
        {
            Id = endpointId,
            Controller = "Test",
            Action = "Get",
            HttpMethod = "GET"
        };

        _endpointRepository.GetEndpointAsync(endpointId).Returns(expectedEndpoint);

        Result<Endpoint, AuthorizationManagementError> result = await _service.GetEndpointAsync(endpointId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedEndpoint);
    }

    [Fact]
    public async Task GetEndpointAsync_RepositoryError_ReturnsError()
    {
        const long endpointId = 123;

        _endpointRepository.GetEndpointAsync(endpointId).Returns(AuthorizationManagementError.EndpointNotFound);

        Result<Endpoint, AuthorizationManagementError> result = await _service.GetEndpointAsync(endpointId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.EndpointNotFound);
    }

    [Fact]
    public async Task GetEndpointsAsync_RepositorySuccess_ReturnsEndpoints()
    {
        Endpoint[] expectedEndpoints =
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

        _endpointRepository.GetEndpointsAsync().Returns(expectedEndpoints);

        Result<IEnumerable<Endpoint>, AuthorizationManagementError> result = await _service.GetEndpointsAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ToArray().ShouldBe(expectedEndpoints);
    }

    [Fact]
    public async Task GetEndpointsAsync_RepositoryError_ReturnsError()
    {
        _endpointRepository.GetEndpointsAsync().Returns(AuthorizationManagementError.Unknown);

        Result<IEnumerable<Endpoint>, AuthorizationManagementError> result = await _service.GetEndpointsAsync();

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.Unknown);
    }

    [Fact]
    public async Task CreateEndpointAsync_ValidParameters_CallsRepository()
    {
        const string controller = "Test";
        const string action = "Action";
        const string httpMethod = "POST";
        Endpoint expectedEndpoint = new()
        {
            Id = 1,
            Controller = controller,
            Action = action,
            HttpMethod = httpMethod
        };

        _endpointRepository.CreateEndpointAsync(controller, action, httpMethod).Returns(expectedEndpoint);

        Result<Endpoint, AuthorizationManagementError> result = await _service.CreateEndpointAsync(controller, action, httpMethod);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedEndpoint);
        await _endpointRepository.Received(1).CreateEndpointAsync(controller, action, httpMethod);
    }

    [Fact]
    public async Task CreateEndpointAsync_ControllerTooLong_ReturnsBadRequest()
    {
        string longController = new('a', 101);
        const string action = "Action";
        const string httpMethod = "POST";

        Result<Endpoint, AuthorizationManagementError> result = await _service.CreateEndpointAsync(longController, action, httpMethod);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.BadRequest);
        await _endpointRepository.DidNotReceive().CreateEndpointAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task CreateEndpointAsync_ActionTooLong_ReturnsBadRequest()
    {
        const string controller = "Test";
        string longAction = new('a', 101);
        const string httpMethod = "POST";

        Result<Endpoint, AuthorizationManagementError> result = await _service.CreateEndpointAsync(controller, longAction, httpMethod);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.BadRequest);
        await _endpointRepository.DidNotReceive().CreateEndpointAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task CreateEndpointAsync_HttpMethodTooLong_ReturnsBadRequest()
    {
        const string controller = "Test";
        const string action = "Action";
        string longHttpMethod = new('a', 101);

        Result<Endpoint, AuthorizationManagementError> result = await _service.CreateEndpointAsync(controller, action, longHttpMethod);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.BadRequest);
        await _endpointRepository.DidNotReceive().CreateEndpointAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task CreateEndpointAsync_RepositoryError_ReturnsError()
    {
        const string controller = "Test";
        const string action = "Action";
        const string httpMethod = "POST";

        _endpointRepository.CreateEndpointAsync(controller, action, httpMethod).Returns(AuthorizationManagementError.EntityAlreadyExists);

        Result<Endpoint, AuthorizationManagementError> result = await _service.CreateEndpointAsync(controller, action, httpMethod);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.EntityAlreadyExists);
    }

    [Fact]
    public async Task DeleteEndpointAsync_RepositorySuccess_ReturnsSuccess()
    {
        const long endpointId = 1;

        _endpointRepository.DeleteEndpointAsync(endpointId).Returns(Result<AuthorizationManagementError>.Success());

        Result<AuthorizationManagementError> result = await _service.DeleteEndpointAsync(endpointId);

        result.IsSuccess.ShouldBeTrue();
        await _endpointRepository.Received(1).DeleteEndpointAsync(endpointId);
    }

    [Fact]
    public async Task DeleteEndpointAsync_RepositoryError_ReturnsError()
    {
        const long endpointId = 123;

        _endpointRepository.DeleteEndpointAsync(endpointId).Returns(AuthorizationManagementError.EndpointNotFound);

        Result<AuthorizationManagementError> result = await _service.DeleteEndpointAsync(endpointId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.EndpointNotFound);
    }

    [Fact]
    public async Task GetEndpointPermissionRequirementsAsync_ByIdRepositorySuccess_ReturnsPermissions()
    {
        const long endpointId = 1;
        Permission[] expectedPermissions =
        [
            new(1, "read.test", "Read test"),
            new(2, "write.test", "Write test")
        ];

        _endpointRepository.GetEndpointPermissionRequirementsAsync(endpointId).Returns(expectedPermissions);

        Result<IEnumerable<Permission>, AuthorizationManagementError> result =
            await _service.GetEndpointPermissionRequirementsAsync(endpointId);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ToArray().ShouldBe(expectedPermissions);
    }

    [Fact]
    public async Task GetEndpointPermissionRequirementsAsync_ByIdRepositoryError_ReturnsError()
    {
        const long endpointId = 123;

        _endpointRepository.GetEndpointPermissionRequirementsAsync(endpointId).Returns(AuthorizationManagementError.EndpointNotFound);

        Result<IEnumerable<Permission>, AuthorizationManagementError> result =
            await _service.GetEndpointPermissionRequirementsAsync(endpointId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.EndpointNotFound);
    }

    [Fact]
    public async Task GetEndpointPermissionRequirementsAsync_ByDetailsRepositorySuccess_ReturnsPermissions()
    {
        const string controller = "Test";
        const string action = "Action";
        const string httpMethod = "GET";
        Permission[] expectedPermissions =
        [
            new(1, "read.test", "Read test"),
            new(2, "write.test", "Write test")
        ];

        _endpointRepository.GetEndpointPermissionRequirementsAsync(controller, action, httpMethod).Returns(expectedPermissions);

        Result<IEnumerable<Permission>, AuthorizationManagementError> result =
            await _service.GetEndpointPermissionRequirementsAsync(controller, action, httpMethod, false);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().ToArray().ShouldBe(expectedPermissions);
    }

    [Fact]
    public async Task GetEndpointPermissionRequirementsAsync_ByDetailsRepositoryError_ReturnsError()
    {
        const string controller = "Unknown";
        const string action = "Action";
        const string httpMethod = "GET";

        _endpointRepository.GetEndpointPermissionRequirementsAsync(controller, action, httpMethod)
            .Returns(AuthorizationManagementError.EndpointNotFound);

        Result<IEnumerable<Permission>, AuthorizationManagementError> result =
            await _service.GetEndpointPermissionRequirementsAsync(controller, action, httpMethod, false);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.EndpointNotFound);
    }

    [Fact]
    public async Task AddPermissionRequirementToEndpointAsync_RepositorySuccess_ReturnsSuccess()
    {
        const long endpointId = 1;
        const long permissionId = 2;

        _endpointRepository.AddPermissionRequirementToEndpointAsync(endpointId, permissionId)
            .Returns(Result<AuthorizationManagementError>.Success());

        Result<AuthorizationManagementError> result = await _service.AddPermissionRequirementToEndpointAsync(endpointId, permissionId);

        result.IsSuccess.ShouldBeTrue();
        await _endpointRepository.Received(1).AddPermissionRequirementToEndpointAsync(endpointId, permissionId);
    }

    [Fact]
    public async Task AddPermissionRequirementToEndpointAsync_RepositoryError_ReturnsError()
    {
        const long endpointId = 123;
        const long permissionId = 2;

        _endpointRepository.AddPermissionRequirementToEndpointAsync(endpointId, permissionId)
            .Returns(AuthorizationManagementError.AnyEntityNotFound);

        Result<AuthorizationManagementError> result = await _service.AddPermissionRequirementToEndpointAsync(endpointId, permissionId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.AnyEntityNotFound);
    }

    [Fact]
    public async Task RemovePermissionRequirementFromEndpointAsync_RepositorySuccess_ReturnsSuccess()
    {
        const long endpointId = 1;
        const long permissionId = 2;

        _endpointRepository.RemovePermissionRequirementFromEndpointAsync(endpointId, permissionId)
            .Returns(Result<AuthorizationManagementError>.Success());

        Result<AuthorizationManagementError> result = await _service.RemovePermissionRequirementFromEndpointAsync(endpointId, permissionId);

        result.IsSuccess.ShouldBeTrue();
        await _endpointRepository.Received(1).RemovePermissionRequirementFromEndpointAsync(endpointId, permissionId);
    }

    [Fact]
    public async Task RemovePermissionRequirementFromEndpointAsync_RepositoryError_ReturnsError()
    {
        const long endpointId = 123;
        const long permissionId = 2;

        _endpointRepository.RemovePermissionRequirementFromEndpointAsync(endpointId, permissionId)
            .Returns(AuthorizationManagementError.AnyEntityNotFound);

        Result<AuthorizationManagementError> result = await _service.RemovePermissionRequirementFromEndpointAsync(endpointId, permissionId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AuthorizationManagementError.AnyEntityNotFound);
    }
}
