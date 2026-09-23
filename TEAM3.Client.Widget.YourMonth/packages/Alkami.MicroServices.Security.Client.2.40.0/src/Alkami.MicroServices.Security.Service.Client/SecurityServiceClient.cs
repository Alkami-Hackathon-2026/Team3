using Alkami.Contracts;
using Alkami.MicroServices.Security.Contracts;
using Alkami.MicroServices.Security.Contracts.Requests;
using Alkami.MicroServices.Security.Contracts.Responses;
using Alkami.MicroServices.Security.Data;
using Alkami.Services.Subscriptions.ParticipatingClient;
using System.Threading.Tasks;

namespace Alkami.MicroServices.Security.Service.Client
{
    /// <inheritdoc cref="ISecurityServiceContract" />
	public class SecurityServiceClient : SelfResolvingClient<ISecurityServiceContract>, ISecurityServiceContract
    {
        /// <inheritdoc />
		public Task<GetUserResponse> GetUserAsync(GetUserRequest request)
		{
			return ProxyCall((c, r) => c.GetUserAsync(r), request);
		}

	    /// <inheritdoc />
	    public Task<GetEntityResponse> GetEntityAsync(GetEntityRequest request)
	    {
	        return ProxyCall((contract, r) => contract.GetEntityAsync(r), request);
	    }

        /// <inheritdoc />
        public Task<GetEntityGroupResponse> GetEntityGroupAsync(GetEntityGroupRequest request)
	    {
            return ProxyCall((contract, r) => contract.GetEntityGroupAsync(r), request);
	    }

        /// <inheritdoc />
	    public Task<GetEntityGroupMemberResponse> GetEntityGroupMemberAsync(GetEntityGroupMemberRequest request)
	    {
	        return ProxyCall((contract, r) => contract.GetEntityGroupMemberAsync(r), request);
	    }

        /// <inheritdoc />
        public Task<BaseResponse<AccessLevel>> GetAccessLevelsAsync(GetAccessLevelRequest request)
		{
			return ProxyCall((c, r) => c.GetAccessLevelsAsync(r), request);
		}

        /// <inheritdoc />
		public Task<BaseResponse<Permission>> GetPermissionsAsync(GetPermissionRequest request)
		{
			return ProxyCall((c, r) => c.GetPermissionsAsync(r), request);
		}

        /// <inheritdoc />
		public Task<BaseResponse<AccessLevel>> AddOrUpdateAccessLevelsAsync(AddOrUpdateAccessLevelsRequest request)
		{
			return ProxyCall((c, r) => c.AddOrUpdateAccessLevelsAsync(r), request);
		}

        /// <inheritdoc />
		public Task<BaseResponse<User>> AddOrUpdateUserAsync(AddOrUpdateUserRequest request)
		{
			return ProxyCall((c, r) => c.AddOrUpdateUserAsync(r), request);
		}

        /// <inheritdoc />
        public Task<DeleteUsersResponse> DeleteUsersAsync(DeleteUsersRequest request)
        {
            return ProxyCall((c, r) => c.DeleteUsersAsync(r), request);
        }

        /// <inheritdoc />
        public Task<BaseResponse<Entity>> AddOrUpdateEntityAsync(AddOrUpdateEntityRequest request)
	    {
	        return ProxyCall((c, r) => c.AddOrUpdateEntityAsync(r), request);
        }

        /// <inheritdoc />
        public Task<BaseResponse<EntityGroup>> AddOrUpdateEntityGroupAsync(AddOrUpdateEntityGroupRequest request)
	    {
	        return ProxyCall((c, r) => c.AddOrUpdateEntityGroupAsync(r), request);
	    }

        /// <inheritdoc />
        public Task<BaseResponse<EntityGroupMember>> AddOrUpdateEntityGroupMemberAsync(AddOrUpdateEntityGroupMemberRequest request)
	    {
	        return ProxyCall((c, r) => c.AddOrUpdateEntityGroupMemberAsync(r), request);
	    }

        /// <inheritdoc />
        public Task<GetFlavorUserMetadataResponse> GetFlavorUserMetadataAsync(GetFlavorUserMetadataRequest request)
        {
            return ProxyCall((c, r) => c.GetFlavorUserMetadataAsync(r), request);
        }

        /// <inheritdoc />
        public Task<GetJointOwnerResponse> GetJointOwnerAsync(GetJointOwnerRequest request)
        {
            return ProxyCall((c, r) => c.GetJointOwnerAsync(r), request);
        }

        /// <inheritdoc />
        public Task<GetUserResponse> GetExternalUserAsync(GetExternalUserRequest request)
        {
            return ProxyCall((c, r) => c.GetExternalUserAsync(r), request);
        }

        /// <inheritdoc />
        public Task<UpdateMarketingOptOutResponse> UpdateMarketingOptOutAsync(UpdateMarketingOptOutRequest request)
		{
			return ProxyCall((c, r) => c.UpdateMarketingOptOutAsync(r), request);
		}

        /// <inheritdoc />
        public Task<GetLoginGroupResponse> GetLoginGroupAsync(GetLoginGroupRequest request)
        {
            return ProxyCall((c, r) => c.GetLoginGroupAsync(r), request);
        }

        /// <inheritdoc />
        public Task<AddOrUpdateLoginGroupResponse> AddOrUpdateLoginGroupAsync(AddOrUpdateLoginGroupRequest request)
        {
            return ProxyCall((c, r) => c.AddOrUpdateLoginGroupAsync(r), request);
        }

        /// <inheritdoc />
        public Task<GetUserResponse> SearchUsersAsync(SearchUsersRequest request)
        {
            return ProxyCall((c, r) => c.SearchUsersAsync(r), request);
        }
    }
}