*** Settings ***
Library         RequestsLibrary
Resource        ../../libs/public-api-common.robot

Force Tags      GeneralPublic    PublicApi    Local    Dev    Test    Preprod

Suite Setup     create session    papi    %{PUBLIC_API_URL}
Test Setup      fail test fast if required


*** Variables ***
${DEFAULT_PAGE_SIZE}=       10


*** Test Cases ***
List publications with a search term to find matching publications returns a non-empty paginated list
    [Tags]    NotAgainstLocal
    &{params}=    create dictionary    search=test    pageSize=${DEFAULT_PAGE_SIZE}
    ${response}=    GET On Session    papi
    ...    url=publications
    ...    params=${params}
    ...    expected_status=ok
    ${response_json}=    response should be json    ${response}
    response should contain pagination info
    ...    ${response_json}
    ...    expected_page_size=${DEFAULT_PAGE_SIZE}
    paginated response should contain non-empty results    ${response_json}

List publications with a short search term less than 3 characters returns a validation error
    [Tags]    NotAgainstLocal
    &{params}=    create dictionary    search=aa
    ${response}=    GET On Session    papi
    ...    url=publications
    ...    params=${params}
    ...    expected_status=bad_request
    ${response_json}=    response should be json    ${response}
    response should contain validation error for path
    ...    ${response_json}
    ...    path=search
    ...    expected_message=Must be at least 3 characters (was 2).
