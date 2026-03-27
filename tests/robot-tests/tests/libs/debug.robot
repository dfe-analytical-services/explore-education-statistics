*** Settings ***
Library     SeleniumLibrary
Library     BuiltIn


*** Keywords ***
Log browser console logs
    ${selenium}=    Get Library Instance    SeleniumLibrary
    ${webdriver}=    Set Variable    ${selenium._drivers.active_drivers}[0]
    ${log_entries}=    Evaluate    $webdriver.get_log('browser')
    Log to console    ${log_entries}

# This keyword will print out all request and response info from Chrome DevTools in
# the browser controlled by Selenium for the lifetime of a test.
# There will be a lot, so some filtering would most likely be needed to be applied
# prior to using this.
#
# A good example of some potential filtering would be to use specific URLs to target
# problem traffic to pages, e.g:
#
# IF    '${message["params"]["request"]["url"]}' == 'https://some-url'

Log network traffic from performance log
    ${sl}=    Get Library Instance    SeleniumLibrary
    ${driver}=    Set Variable    ${sl.driver}
    ${entries}=    Call Method    ${driver}    get_log    performance

    FOR    ${entry}    IN    @{entries}
        ${message}=    Evaluate    json.loads($entry["message"])["message"]    json

        IF    '${message["method"]}' == 'Network.requestWillBeSent'
            Log to console    URL: ${message["params"]["request"]["url"]}
            Log to console    Request ID: ${message["params"]["requestId"]}
        END

        IF    '${message["method"]}' == 'Network.responseReceived'
            ${headers}=    Evaluate    json.dumps(${message["params"]["response"]["headers"]}, indent=2)
            ...    modules=json

            Log to console    HTTP response to requestId: ${message["params"]["requestId"]}
            Log to console    HTTP response URL: ${message["params"]["response"]["url"]}
            Log to console    HTTP response status: ${message["params"]["response"]["status"]}
            Log to console    HTTP response headers:
            Log to console    ${headers}
        END

        IF    '${message["method"]}' == 'Network.requestWillBeSentExtraInfo'
            ${headers}=    Evaluate    json.dumps(${message["params"]["headers"]}, indent=2)    modules=json

            Log to console    HTTP request headers for requestId ${message["params"]["requestId"]}
            Log to console    ${headers}

            ${has_client_security_state}=    Run Keyword And Return Status
            ...    Dictionary Should Contain Key    ${message["params"]}    clientSecurityState

            IF    ${has_client_security_state}
                ${security_state}=    Evaluate    json.dumps(${message["params"]["clientSecurityState"]}, indent=2)
                ...    modules=json
                Log to console    HTTP request client security state:
                Log to console    ${security_state}
            END

            Log to console    \n\n\n
        END
    END
