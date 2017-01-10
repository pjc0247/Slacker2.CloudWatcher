using System;

using Slacker2;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.SQS;
using Amazon.SQS.Model;

    public class CloudWatchAlarm
	{
		public string TopicArn { get; set; }

		public string Name { get; set; }
		public string Reason { get; set; }
	}
	public class CloudWatcher : BotService
	{
		private AmazonSQSClient Client { get; }
		private string QueueID { get; }

		public CloudWatcher()
		{
			QueueID = "https://sqs.us-west-2.amazonaws.com/457272141675/test";
			Client = new AmazonSQSClient();
		}

		[Schedule(10)]
		public void OnSchedule()
		{
			var messageResponse = Client.ReceiveMessage(
				new ReceiveMessageRequest()
				{
					QueueUrl = QueueID,
					MaxNumberOfMessages = 10,
					VisibilityTimeout = 1
				});

			if (messageResponse.HttpStatusCode == HttpStatusCode.OK &&
				messageResponse.Messages.Count > 0)
			{
				foreach (var msg in messageResponse.Messages)
				{
					var jobj = JObject.Parse(msg.Body);

					var topicArn = jobj["TopicArn"].Value<string>();
					var body = jobj["Message"].Value<string>();

					var alarmObj = JObject.Parse(body);
					var alarmName = alarmObj["AlarmName"].Value<string>();
					var alarmReason = alarmObj["NewStateReason"].Value<string>();

					OnReceivedAlarm(new CloudWatchAlarm()
					{
						Name = alarmName,
						Reason = alarmReason,

						TopicArn = topicArn
					});

					Client.DeleteMessage(QueueID, msg.ReceiptHandle);
				}
			}
		}

		protected virtual void OnReceivedAlarm(CloudWatchAlarm alarm)
		{
			
		}
	}
