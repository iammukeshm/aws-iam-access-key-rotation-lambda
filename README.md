# Automated AWS IAM Access Key Rotation
## with AWS Lambda, EventBridge Scheduler, SNS, and .NET!

![_AWS IAM Access Key Rotation Lambda](https://github.com/iammukeshm/aws-iam-access-key-rotation-lambda/assets/31455818/17187491-b6f3-42b7-826c-396a769beaef)

Proposed Workflow:

1. Gets a list of users and iterates through each of them.

2. Gets a list of access keys attached to the user and iterates through it,

3. Calculate the age of each key.

4. If the key age is within 60 - 70 days and is in an active state, we will create a new access key. A notification email will be sent to the user via SNS Topic Subscription.

5. If the key age is within 80 - 90 days, we will consider it eligible for deactivation. The keys will be deactivated and the notification will be sent over to the user.

6. If the key age is above 90 days, the keys will be deactivated and the notifications will be sent to the user.

7. Using Amazon Event Bridge Scheduler, we will have to ensure that the Lambda is scheduled to run every week.

![AWS IAM Access Key Rotation Lambda Workflow](https://github.com/iammukeshm/aws-iam-access-key-rotation-lambda/assets/31455818/1434bfb8-32f0-418a-bd53-34adfd2cf2da)

This ensures that your Access Keys are always secured and rotated. The Complete Source code is attached to the article!

Read: https://codewithmukesh.com/blog/automated-aws-iam-access-key-rotation/

## Subscribe to my YouTube Channel ❤️

Do support me by subscribing to my YouTube channel for more .NET Content!

https://www.youtube.com/@codewithmukesh/
