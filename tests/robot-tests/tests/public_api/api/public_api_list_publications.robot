*** Settings ***
Library         ../../libs/public_api.py
Resource        ../../libs/public-api-common.robot

Force Tags      GeneralPublic    PublicApi    Local    Dev    Test    Preprod

Test Setup      fail test fast if required


*** Variables ***
${DEFAULT_PAGE_SIZE}=       10


*** Test Cases ***
List publications with a search term to find matching publications returns a non-empty paginated list
    [Tags]    NotAgainstLocal
    &{params}=    create dictionary    search=test    pageSize=${DEFAULT_PAGE_SIZE}
    ${response}=    user makes get request to public api    /publications    params=${params}
    user checks response status code    ${response}    200
    ${response_json}=    response should be json    ${response}
    response should contain pagination info
    ...    ${response_json}
    ...    expected_page_size=${DEFAULT_PAGE_SIZE}
    paginated response should contain non-empty results    ${response_json}

List publications with a short search term less than 3 characters returns a validation error
    [Tags]    NotAgainstLocal
    &{params}=    create dictionary    search=aa
    ${response}=    user makes get request to public api    /publications    params=${params}
    user checks response status code    ${response}    400
    ${response_json}=    response should be json    ${response}
    response should contain validation error for path
    ...    ${response_json}
    ...    path=search
    ...    expected_message=Must be at least 3 characters (was 2).

List publications returns a non-empty paginated list
    [Tags]    NotAgainstLocal
    &{params}=    create dictionary    pageSize=${DEFAULT_PAGE_SIZE}
    ${response}=    user makes get request to public api    /publications    params=${params}
    user checks response status code    ${response}    200
    ${response_json}=    response should be json    ${response}
    response should contain pagination info
    ...    ${response_json}
    ...    expected_page_size=${DEFAULT_PAGE_SIZE}
    paginated response should contain non-empty results    ${response_json}

    ${PUBLICATION_ID}=    set variable    ${response_json['results'][0]['id']}
    set suite variable    ${PUBLICATION_ID}

Get a publication returns the publication
    [Tags]    NotAgainstLocal
    ${response}=    user makes get request to public api    /publications/${PUBLICATION_ID}
    user checks response status code    ${response}    200
    ${response_json}=    response should be json    ${response}
    should be equal    ${response_json['id']}    ${PUBLICATION_ID}

List a publication's data sets returns a non-empty paginated list
    [Tags]    NotAgainstLocal
    ${response}=    user makes get request to public api    /publications/${PUBLICATION_ID}/data-sets
    user checks response status code    ${response}    200
    ${response_json}=    response should be json    ${response}
    paginated response should contain non-empty results    ${response_json}

    # Every publication in PAPI will have at least one data set associated with it.
    ${DATA_SET_ID}=    set variable    ${response_json['results'][0]['id']}
    set suite variable    ${DATA_SET_ID}

Get a data set returns the data set with a latest version
    [Tags]    NotAgainstLocal
    ${response}=    user makes get request to public api    /data-sets/${DATA_SET_ID}
    user checks response status code    ${response}    200
    ${response_json}=    response should be json    ${response}
    should be equal    ${response_json['id']}    ${DATA_SET_ID}
    should be equal    ${response_json['status']}    Published

    ${DATA_SET_VERSION}=    set variable    ${response_json['latestVersion']['version']}
    set suite variable    ${DATA_SET_VERSION}

Get a data set's meta returns the data set meta
    [Tags]    NotAgainstLocal
    ${response}=    user makes get request to public api    /data-sets/${DATA_SET_ID}/meta
    user checks response status code    ${response}    200
    ${response_json}=    response should be json    ${response}
    should not be empty    ${response_json['indicators']}
    should not be empty    ${response_json['timePeriods']}

    ${INDICATOR_ID}=    set variable    ${response_json['indicators'][0]['id']}
    set suite variable    ${INDICATOR_ID}

Query a data set returns a non-empty paginated list of results
    [Tags]    NotAgainstLocal
    ${response}=    user makes post request to public api    /data-sets/${DATA_SET_ID}/query
    ...    body={"indicators":["${INDICATOR_ID}"],"pageSize":1}
    user checks response status code    ${response}    200
    ${response_json}=    response should be json    ${response}
    response should contain pagination info    ${response_json}    expected_page_size=1
    paginated response should contain non-empty results    ${response_json}
    dictionary should contain key    ${response_json['results'][0]['values']}    ${INDICATOR_ID}

List a data set's versions returns a non-empty paginated list
    [Tags]    NotAgainstLocal
    ${response}=    user makes get request to public api    /data-sets/${DATA_SET_ID}/versions
    user checks response status code    ${response}    200
    ${response_json}=    response should be json    ${response}
    paginated response should contain non-empty results    ${response_json}

Get a data set version returns the version
    [Tags]    NotAgainstLocal
    ${response}=    user makes get request to public api
    ...    /data-sets/${DATA_SET_ID}/versions/${DATA_SET_VERSION}
    user checks response status code    ${response}    200
    ${response_json}=    response should be json    ${response}
    should be equal    ${response_json['version']}    ${DATA_SET_VERSION}
    should be equal    ${response_json['status']}    Published
