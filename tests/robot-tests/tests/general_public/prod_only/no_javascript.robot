*** Settings ***
Resource        ../../libs/public-common.robot
Library         ../../libs/no_javascript.py

Test Setup      fail test fast if required

Force Tags      GeneralPublic    Prod


*** Test Cases ***
Parse Find Statistics page HTML
    [Documentation]    EES-1186
    # Search by keywords here so Pupil absence in schools in England is on the page.
    ${parsed_page}=    user gets parsed html from page
    ...    %{PUBLIC_URL}/find-statistics?sortBy=relevance&search=Pupil+absence+in+schools+in+England
    set suite variable    ${parsed_page}

Validate Seed Data Theme 1 Publication 1 publication on page
    ${list}=    user_gets_publications_list    ${parsed_page}

    Skip    Skipping as not compatible with azure search
    user_checks_list_contains_publication    ${list}    Pupil absence in schools in England
