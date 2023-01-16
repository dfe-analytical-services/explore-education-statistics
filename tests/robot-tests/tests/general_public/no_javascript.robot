*** Settings ***
Resource        ../libs/public-common.robot
Library         ../libs/no_javascript.py

Test Setup      fail test fast if required

Force Tags      GeneralPublic    Local    Dev    Test    Preprod    Prod


*** Test Cases ***
Parse Find Statistics page HTML
    [Documentation]    EES-1186
    # sortBy oldest here so Pupil absence is on the page.
    ${parsed_page}=    user gets parsed html from page    %{PUBLIC_URL}/find-statistics?sortBy=oldest
    set suite variable    ${parsed_page}

Validate Pupils absence publication on page
    ${list}=    user_gets_publications_list    ${parsed_page}
    user_checks_list_contains_publication    ${list}    Pupil absence in schools in England
