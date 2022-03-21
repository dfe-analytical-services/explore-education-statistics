import os
from slack_sdk.webhook import WebhookClient


def send_slack_alert(err_msg):
    url = os.getenv('SLACK_WEBHOOK_URL')
    assert url is not None or not "", 'SLACK_WEBHOOK_URL env variable needs to be set'
    slack_webhook = WebhookClient(url)
    response = slack_webhook.send(text=err_msg)
    assert response.status_code == 200
    assert response.body == "ok"
