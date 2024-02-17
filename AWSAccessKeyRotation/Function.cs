using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSAccessKeyRotation;

public class Function
{
    private static readonly string TopicArn = "arn:aws:sns:us-east-1:821175633958:access-key-rotation";
    private static readonly IAmazonIdentityManagementService iamClient = new AmazonIdentityManagementServiceClient();
    private static readonly IAmazonSimpleNotificationService snsClient = new AmazonSimpleNotificationServiceClient();
    public async Task FunctionHandler(ILambdaContext context)
    {
        var usersResponse = await iamClient.ListUsersAsync();
        var expiryThreshold = 40;
        var deactivationThreshold = 20;
        foreach (var user in usersResponse.Users)
        {
            var accessKeyRequst = new ListAccessKeysRequest()
            {
                UserName = user.UserName
            };
            var accessKeyDetails = await iamClient.ListAccessKeysAsync(accessKeyRequst);
            foreach (var accessKey in accessKeyDetails.AccessKeyMetadata)
            {
                var createdDate = accessKey.CreateDate;
                var keyAge = DateTime.Now - createdDate;
                var keyAgeInDays = keyAge.Days;

                if (keyAgeInDays >= deactivationThreshold - 10
                    && keyAgeInDays < deactivationThreshold
                    && accessKeyDetails.AccessKeyMetadata.Count == 1
                    && accessKey.Status == StatusType.Active)
                {
                    //eligible to create new access key.
                    await CreateAccessKey(user.UserName);
                }
                else if (keyAgeInDays >= expiryThreshold - 10
                    && keyAgeInDays < expiryThreshold
                    && accessKey.Status == StatusType.Active)
                {
                    //eligible for deactivation
                    await DeactivateAccessKey(user.UserName, accessKey.AccessKeyId);
                }
                else if (keyAgeInDays >= expiryThreshold && accessKey.Status == StatusType.Inactive)
                {
                    //expired and inactive, so delete
                    await DeleteAccessKey(user.UserName, accessKey.AccessKeyId);

                }

            }
        }
    }

    public async Task CreateAccessKey(string userName)
    {
        var request = new CreateAccessKeyRequest()
        {
            UserName = userName
        };
        var keyDetails = await iamClient.CreateAccessKeyAsync(request);
        var message = $"New Access Key Generated for user : {userName}. " +
            $"New Access Key Id : {keyDetails.AccessKey.AccessKeyId}. " +
            $"New Secret Access Key : {keyDetails.AccessKey.SecretAccessKey}." +
            $"Old Key will be Deactivated in about 10 days.";
        var publishRequest = new PublishRequest()
        {
            TopicArn = TopicArn,
            Message = message
        };
        await snsClient.PublishAsync(publishRequest);

    }

    public async Task DeleteAccessKey(string userName, string accessKeyId)
    {
        var request = new DeleteAccessKeyRequest()
        {
            UserName = userName,
            AccessKeyId = accessKeyId
        };
        await iamClient.DeleteAccessKeyAsync(request);
        var message = $"Access Key : {accessKeyId} has been deleted for user : {userName}.";
        var publishRequest = new PublishRequest()
        {
            TopicArn = TopicArn,
            Message = message
        };
        await snsClient.PublishAsync(publishRequest);
    }

    public async Task DeactivateAccessKey(string userName, string accessKeyId)
    {
        var request = new UpdateAccessKeyRequest()
        {
            UserName = userName,
            AccessKeyId = accessKeyId,
            Status = StatusType.Inactive
        };
        await iamClient.UpdateAccessKeyAsync(request);
        var message = $"Access Key : {accessKeyId} has been deactivated for user : {userName}.";
        var publishRequest = new PublishRequest()
        {
            TopicArn = TopicArn,
            Message = message
        };
        await snsClient.PublishAsync(publishRequest);
    }
}
