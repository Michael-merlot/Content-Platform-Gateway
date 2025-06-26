using Gateway.Api.Auth;
using Gateway.Api.Mappers;
using Gateway.Api.Models.Auth;
using Gateway.Core.Interfaces.Auth;
using Gateway.Core.Models;
using Gateway.Core.Models.Auth;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;

using Endpoint = Gateway.Core.Models.Auth.Endpoint;

namespace Gateway.Api.Controllers.Auth;

[ApiController]
[Route("/api/v1/auth")]
[AdminUntilDynamicAuthorize]
public class AuthorizationManagementController : ControllerBase
{
    private readonly IAuthorizationManagementService _authorizationManagementService;
    private readonly IEnumerable<EndpointDataSource> _endpointSources;

    public AuthorizationManagementController(IAuthorizationManagementService authorizationManagementService,
        IEnumerable<EndpointDataSource> endpointSources)
    {
        _authorizationManagementService = authorizationManagementService;
        _endpointSources = endpointSources;
    }

    /// <summary>Gets the role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>The requested role.</returns>
    /// <response code="200">The requested role.</response>
    /// <response code="404">The role has not been found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("roles/{roleId:long}")]
    [ProducesResponseType<RoleAdminDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> GetRole(long roleId)
    {
        Result<Role, AuthorizationManagementError> result = await _authorizationManagementService.GetRoleAsync(roleId);

        return result.Match(value => Ok(value.ToAdminDto()),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Gets all roles.</summary>
    /// <returns>The collection of roles.</returns>
    /// <response code="200">The collection of roles.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("roles")]
    [ProducesResponseType<RoleCollectionAdminResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> GetRoles()
    {
        Result<IEnumerable<Role>, AuthorizationManagementError> result = await _authorizationManagementService.GetRolesAsync();

        return result.Match(value => Ok(value.ToAdminDto()),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Creates a role.</summary>
    /// <param name="request">The request to create a role.</param>
    /// <returns>The created role.</returns>
    /// <response code="200">The created role.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="409">The role already exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("roles")]
    [ProducesResponseType<RoleAdminDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> CreateRole(CreateRoleRequest request)
    {
        Result<Role, AuthorizationManagementError> result = await _authorizationManagementService.CreateRoleAsync(request.Name);

        return result.Match(value => CreatedAtAction(nameof(GetRole), new
            {
                roleId = value.Id
            }, value),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Deletes the role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns></returns>
    /// <response code="204">The role was successfully deleted.</response>
    /// <response code="404">The role has not been found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("roles/{roleId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> DeleteRole(long roleId)
    {
        Result<AuthorizationManagementError> result = await _authorizationManagementService.DeleteRoleAsync(roleId);

        return result.Match(NoContent,
            error => error.ToProblemDetails(this));
    }

    /// <summary>Gets permissions of the role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns>The collection of role permissions.</returns>
    /// <response code="200">The collection of role permissions.</response>
    /// <response code="404">The role has not been found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("roles/{roleId:long}/permissions")]
    [ProducesResponseType<PermissionCollectionResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> GetRolePermissions(long roleId)
    {
        Result<IEnumerable<Permission>, AuthorizationManagementError> result =
            await _authorizationManagementService.GetRolePermissionsAsync(roleId);

        return result.Match(value => Ok(value.ToDto()),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Assigns permission to the role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="request">The request to assign the permission.</param>
    /// <returns></returns>
    /// <response code="204">The permission was successfully assigned.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">The role or permission was not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("roles/{roleId:long}/permissions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> AddPermissionToRole(long roleId, AddPermissionToRoleRequest request)
    {
        Result<AuthorizationManagementError> result =
            await _authorizationManagementService.AddPermissionToRoleAsync(roleId, request.PermissionId);

        return result.Match(NoContent,
            error => error.ToProblemDetails(this));
    }

    /// <summary>Removes permission assignment from the role.</summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns></returns>
    /// <response code="204">The permission assignment was successfully removed.</response>
    /// <response code="404">The role or permission was not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("roles/{roleId:long}/permissions/{permissionId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> RemovePermissionFromRole(long roleId, long permissionId)
    {
        Result<AuthorizationManagementError> result =
            await _authorizationManagementService.RemovePermissionFromRoleAsync(roleId, permissionId);

        return result.Match(NoContent,
            error => error.ToProblemDetails(this));
    }

    /// <summary>Gets user roles.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The collection of user roles.</returns>
    /// <response code="200">The collection of user roles.</response>
    /// <response code="404">The user was not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("users/{userId:int}/roles")]
    [ProducesResponseType<RoleCollectionAdminResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> GetUserRoles(int userId)
    {
        Result<IEnumerable<Role>, AuthorizationManagementError> result = await _authorizationManagementService.GetUserRolesAsync(userId);

        return result.Match(value => Ok(value.ToAdminDto()),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Adds the role to the user.</summary>
    /// <param name="userId">The unique identifier of the role.</param>
    /// <param name="request">The request to add role to the user.</param>
    /// <returns></returns>
    /// <response code="204">The role was successfully added.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">The user or role was not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("users/{userId:int}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> AddRoleToUser(int userId, AddRoleToUserRequest request)
    {
        Result<AuthorizationManagementError> result = await _authorizationManagementService.AddRoleToUserAsync(userId, request.RoleId);

        return result.Match(NoContent,
            error => error.ToProblemDetails(this));
    }

    /// <summary>Removes the role from the user.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <returns></returns>
    /// <response code="204">The role was removed successfully.</response>
    /// <response code="404">The user or role was not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("users/{userId:int}/roles/{roleId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> RemoveRoleFromUser(int userId, long roleId)
    {
        Result<AuthorizationManagementError> result = await _authorizationManagementService.RemoveRoleFromUserAsync(userId, roleId);

        return result.Match(NoContent,
            error => error.ToProblemDetails(this));
    }

    /// <summary>Gets the permission.</summary>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns>The requested permission.</returns>
    /// <response code="200">The requested permission.</response>
    /// <response code="404">The permission was not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("permissions/{permissionId:long}")]
    [ProducesResponseType<PermissionDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> GetPermission(long permissionId)
    {
        Result<Permission, AuthorizationManagementError> result = await _authorizationManagementService.GetPermissionAsync(permissionId);

        return result.Match(value => Ok(value.ToDto()),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Gets all permissions.</summary>
    /// <returns>The collection of all permissions.</returns>
    /// <response code="200">The collection of all permissions.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("permissions")]
    [ProducesResponseType<PermissionCollectionResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> GetPermissions()
    {
        Result<IEnumerable<Permission>, AuthorizationManagementError> result = await _authorizationManagementService.GetPermissionsAsync();

        return result.Match(value => Ok(value.ToDto()),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Create a permission.</summary>
    /// <param name="request">The create permission request.</param>
    /// <returns>The created permission.</returns>
    /// <response code="200">The created permission.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="409">The permission already exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("permissions")]
    [ProducesResponseType<PermissionDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> CreatePermission(CreatePermissionRequest request)
    {
        Result<Permission, AuthorizationManagementError> result =
            await _authorizationManagementService.CreatePermissionAsync(request.Name, request.Description);

        return result.Match(value => CreatedAtAction(nameof(GetPermission), new
            {
                permissionId = value.Id
            }, value),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Deletes the permission.</summary>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns></returns>
    /// <response code="204">The permission was successfully deleted.</response>
    /// <response code="404">The permission was not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("permissions/{permissionId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> DeletePermission(long permissionId)
    {
        Result<AuthorizationManagementError> result = await _authorizationManagementService.DeletePermissionAsync(permissionId);

        return result.Match(NoContent,
            error => error.ToProblemDetails(this));
    }

    /// <summary>Gets the endpoint.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <returns>The requested endpoint.</returns>
    /// <response code="200">The requested endpoint.</response>
    /// <response code="404">The endpoint was not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("endpoints/{endpointId:long}")]
    [ProducesResponseType<EndpointDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> GetEndpoint(long endpointId)
    {
        Result<Endpoint, AuthorizationManagementError> result = await _authorizationManagementService.GetEndpointAsync(endpointId);

        return result.Match(value => Ok(value.ToDto()),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Gets all endpoints.</summary>
    /// <returns>The collection of all endpoints.</returns>
    /// <response code="200">The collection of all endpoints.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("endpoints")]
    [ProducesResponseType<EndpointCollectionResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> GetEndpoints()
    {
        Result<IEnumerable<Endpoint>, AuthorizationManagementError> result = await _authorizationManagementService.GetEndpointsAsync();

        return result.Match(value => Ok(value.ToDto()),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Gets all framework endpoints defined on the server.</summary>
    /// <returns>The collection of framework endpoints.</returns>
    /// <response code="200">The collection of framework endpoints.</response>
    [HttpGet("endpoints/framework")]
    [ProducesResponseType<FrameworkEndpointCollectionResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ExcludeFromCodeCoverage]
    public IActionResult GetFrameworkEndpoints() =>
        Ok(_endpointSources.SelectMany(x => x.Endpoints)
            .Select(x => (Cad: x.Metadata.GetMetadata<ControllerActionDescriptor>(), Hmm: x.Metadata.GetMetadata<HttpMethodMetadata>()))
            .Where(x => x.Cad is not null && x.Hmm is not null)
            .SelectMany(x => x.Hmm!.HttpMethods,
                (metadata, httpMethod) => new FrameworkEndpointDto(metadata.Cad!.ControllerName, metadata.Cad.ActionName, httpMethod))
            .ToDto());

    /// <summary>Creates an endpoint.</summary>
    /// <param name="request">The create endpoint request.</param>
    /// <returns>The created endpoint.</returns>
    /// <response code="200">The created endpoint.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="409">The endpoint already exists.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("endpoints")]
    [ProducesResponseType<EndpointDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> CreateEndpoint(CreateEndpointRequest request)
    {
        Result<Endpoint, AuthorizationManagementError> result =
            await _authorizationManagementService.CreateEndpointAsync(request.Controller, request.Action, request.HttpMethod);

        return result.Match(value => CreatedAtAction(nameof(GetEndpoint), new
            {
                endpointId = value.Id
            }, value),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Deletes the endpoint.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <returns></returns>
    /// <response code="204">The endpoint was successfully deleted.</response>
    /// <response code="404">The endpoint was not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("endpoints/{endpointId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> DeleteEndpoint(long endpointId)
    {
        Result<AuthorizationManagementError> result = await _authorizationManagementService.DeleteEndpointAsync(endpointId);

        return result.Match(NoContent,
            error => error.ToProblemDetails(this));
    }

    /// <summary>Gets endpoint permission requirements.</summary>
    /// <param name="endpointId">The unique identifier of an endpoint.</param>
    /// <returns>The collection of required permissions.</returns>
    /// <response code="200">The collection of required permissions.</response>
    /// <response code="404">The endpoint was not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("endpoints/{endpointId:long}/permissions")]
    [ProducesResponseType<PermissionCollectionResponse>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> GetEndpointPermissionRequirements(long endpointId)
    {
        Result<IEnumerable<Permission>, AuthorizationManagementError> result =
            await _authorizationManagementService.GetEndpointPermissionRequirementsAsync(endpointId);

        return result.Match(value => Ok(value.ToDto()),
            error => error.ToProblemDetails(this));
    }

    /// <summary>Adds permission requirement to the endpoint.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <param name="request">The request to add a permission requirement.</param>
    /// <returns></returns>
    /// <response code="204">The permission requirement was successfully added.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">The endpoint or permission was not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("endpoints/{endpointId:long}/permissions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> AddPermissionRequirementToEndpoint(long endpointId, AddPermissionRequirementToEndpointRequest request)
    {
        Result<AuthorizationManagementError> result =
            await _authorizationManagementService.AddPermissionRequirementToEndpointAsync(endpointId, request.PermissionId);

        return result.Match(NoContent,
            error => error.ToProblemDetails(this));
    }

    /// <summary>Removes permission requirement from the endpoint.</summary>
    /// <param name="endpointId">The unique identifier of the endpoint.</param>
    /// <param name="permissionId">The unique identifier of the permission.</param>
    /// <returns></returns>
    /// <response code="204">The permission requirement was successfully removed.</response>
    /// <response code="404">The endpoint or permission was not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("endpoints/{endpointId:long}/permissions/{permissionId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> RemovePermissionRequirementFromEndpoint(long endpointId, long permissionId)
    {
        Result<AuthorizationManagementError> result =
            await _authorizationManagementService.RemovePermissionRequirementFromEndpointAsync(endpointId, permissionId);

        return result.Match(NoContent,
            error => error.ToProblemDetails(this));
    }
}
