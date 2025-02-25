*** Keywords ***
Log browser console logs
    ${selenium}=    Get Library Instance    SeleniumLibrary
    ${webdriver}=    Set Variable    ${selenium._drivers.active_drivers}[0]
    ${log_entries}=    Evaluate    $webdriver.get_log('browser')
    Log to console    ${log_entries}
