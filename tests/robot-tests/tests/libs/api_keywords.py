import os
import json
import requests

from robot.libraries.BuiltIn import BuiltIn
sl = BuiltIn().get_library_instance('SeleniumLibrary')

def user_creates_test_publication_via_api(publication_name):
    assert os.getenv('TEST_TOPIC_ID') is not None
    assert os.getenv('ADMIN_URL') is not None
    assert os.getenv('IDENTITY_LOCAL_STORAGE_ADMIN') is not None

    topic_id = os.getenv('TEST_TOPIC_ID')
    jwt_token = json.loads(os.getenv('IDENTITY_LOCAL_STORAGE_ADMIN'))['access_token']

    response = requests.post(
        f'{os.getenv("ADMIN_URL")}/api/publications',
        headers={
            'Content-Type': 'application/json',
            'Authorization': f'Bearer {jwt_token}',
        },
        data=json.dumps({
            "title": publication_name,
            "topicId": topic_id,
            "contact": {
                "contactName": "UI test contact name",
                "contactTelNo": "1234 1234",
                "teamEmail": "ui_test@test.com",
                "teamName": "UI test team name",
            },
        }),
        verify=False,
    )
    assert response.status_code < 300, f'Creating publication API request failed with {response.status_code} and {response.text}'
    return response.json()['id']


def user_create_test_release_via_api(publication_id, time_period, year):
    assert os.getenv('ADMIN_URL') is not None
    assert os.getenv('IDENTITY_LOCAL_STORAGE_ADMIN') is not None

    jwt_token = json.loads(os.getenv('IDENTITY_LOCAL_STORAGE_ADMIN'))['access_token']

    response = requests.post(
        f'{os.getenv("ADMIN_URL")}/api/publications/{publication_id}/releases',
        headers={
            'Content-Type': 'application/json',
            'Authorization': f'Bearer {jwt_token}',
        },
        data=json.dumps({
            "publicationId": publication_id,
            "timePeriodCoverage": {
                "value": time_period,
            },
            "releaseName": int(year),
            "typeId": "8becd272-1100-4e33-8a7d-1c0c4e3b42b8",
            "templateReleaseId": "",
        }),
        verify=False,
    )
    assert response.status_code < 300, f'Creating release API request failed with {response.status_code} and {response.text}'
