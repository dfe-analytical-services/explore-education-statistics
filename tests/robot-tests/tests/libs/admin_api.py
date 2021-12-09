import os
import json
import requests

# To prevent InsecureRequestWarning
requests.packages.urllib3.disable_warnings()


class AdminClient:
    ROBOT_AUTO_KEYWORDS = False

    @staticmethod
    def __request(method: str, url: str, body: object = None):
        assert method and url
        assert os.getenv('IDENTITY_LOCAL_STORAGE_ADMIN') is not None
        assert os.getenv('ADMIN_URL') is not None

        requests.sessions.HTTPAdapter(
        pool_connections=50,
        pool_maxsize=50,
        max_retries=3
        )
        session = requests.Session()

        jwt_token = json.loads(os.getenv('IDENTITY_LOCAL_STORAGE_ADMIN'))['access_token']
        headers = {
            'Content-Type': 'application/json',
            'Authorization': f'Bearer {jwt_token}',
        }

        return session.request(
            method,
            url=f'{os.getenv("ADMIN_URL")}{url}',
            headers=headers,
            stream=True,
            json=body,
            verify=False
        )

    def get(self, url: str):
        return self.__request('GET', url)

    def post(self, url: str, body: object = None):
        return self.__request('POST', url, body)

    def put(self, url: str, body: object = None):
        return self.__request('PUT', url, body)

    def patch(self, url: str, body: object = None):
        return self.__request('PATCH', url, body)

    def delete(self, url: str):
        return self.__request('DELETE', url)


admin_client = AdminClient()

def user_creates_theme_via_api(title: str, summary: str = '') -> str:
    assert title

    resp = admin_client.post(f'/api/themes', {
        'title': title,
        'summary': summary
    })

    assert resp.status_code == 200, \
        f'Could not create theme! Responded with {resp.status_code} and {resp.text}'

    return resp.json()['id']


def user_deletes_theme_via_api(theme_id: str):
    assert theme_id

    resp = admin_client.delete(f'/api/themes/{theme_id}')

    assert resp.status_code == 204, \
        f'Could not delete theme! Responded with {resp.status_code} and {resp.text}'


def user_creates_topic_via_api(title: str, theme_id: str) -> str:
    assert title

    resp = admin_client.post(f'/api/topics', {
        'title': title,
        'themeId': theme_id
    })

    assert resp.status_code == 200, \
        f'Could not create topic! Responded with {resp.status_code} and {resp.text}'

    return resp.json()['id']


def user_triggers_release_on_demand(release_id: str):
    resp = admin_client.put(f'/api/bau/release/{release_id}/publish')
    assert resp.status_code == 200, \
        f'Release on demand request failed! Responded with {resp.status_code} and {resp.text}'


def user_creates_test_publication_via_api(publication_name: str, topic_id: str = None):
    if topic_id is not None:
        chosen_topic_id = topic_id
    else:
        assert os.getenv('TEST_TOPIC_ID') is not None
        chosen_topic_id = os.getenv('TEST_TOPIC_ID')

    response = admin_client.post(
        '/api/publications',
        {
            "title": publication_name,
            "topicId": chosen_topic_id,
            "contact": {
                "contactName": "UI test contact name",
                "contactTelNo": "1234 1234",
                "teamEmail": "ui_test@test.com",
                "teamName": "UI test team name",
            },
        }
    )

    assert response.status_code < 300, \
        f'Creating publication API request failed with {response.status_code} and {response.text}'
    return response.json()['id']


def user_create_test_release_via_api(publication_id: str, time_period: str, year: str):
    response = admin_client.post(
        f'/api/publications/{publication_id}/releases',
        {
            "publicationId": publication_id,
            "timePeriodCoverage": {
                "value": time_period,
            },
            "releaseName": int(year),
            "typeId": "8becd272-1100-4e33-8a7d-1c0c4e3b42b8",
            "templateReleaseId": "",
        }
    )

    assert response.status_code < 300, \
        f'Creating release API request failed with {response.status_code} and {response.text}'
