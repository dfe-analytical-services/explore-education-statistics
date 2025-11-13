*** Settings ***
Resource    ./common.robot
Library     admin-utilities.py


*** Keywords ***
response should be json
    [Arguments]    ${response}
    should be equal as strings    ${response.headers['Content-Type']}    application/json; charset=utf-8
    ...    ignore_case=True
    log    ${response.json()}
    RETURN    ${response.json()}

response should contain pagination info
    [Arguments]    ${response_json}    ${expected_page_size}    ${expected_page}=1
    ${paging}=    get from dictionary    ${response_json}    paging
    should be equal as integers    ${paging['pageSize']}    ${expected_page_size}
    should be equal as integers    ${paging['page']}    ${expected_page}
    dictionary should contain key    ${paging}    totalResults
    dictionary should contain key    ${paging}    totalPages

response should contain validation error for path
    [Arguments]    ${response_json}    ${path}    ${expected_message}
    should be equal    ${response_json['title']}    One or more validation errors occurred.
    FOR    ${error}    IN    @{response_json['errors']}
        IF    '${error['path']}' == '${path}'
            should be equal    ${error['message']}    ${expected_message}
            RETURN
        END
    END
    Fail    No validation error found for path '${path}'

paginated response should contain non-empty results
    [Arguments]    ${response_json}
    ${paging}=    get from dictionary    ${response_json}    paging
    should be true    ${paging['totalResults']} > 0
    should be true    ${paging['totalPages']} > 0

paginated response should contain results
    [Arguments]    ${response_json}    ${expected_total_results}    ${expected_total_pages}=-1
    ${paging}=    get from dictionary    ${response_json}    paging
    should be equal as integers    ${paging['totalResults']}    ${expected_total_results}
    IF    ${expected_total_pages} >= 0
        should be equal as integers    ${paging['totalPages']}    ${expected_total_pages}
    ELSE
        should be true    ${paging['totalPages']} > 0
    END

user waits until draft API data set status contains
    [Arguments]    ${expected_status}    ${retries}=10x    ${interval}=%{WAIT_SMALL}s
    wait until keyword succeeds    ${retries}    ${interval}
    ...    user checks summary list contains    Status    ${expected_status}    testid:draft-version-summary
