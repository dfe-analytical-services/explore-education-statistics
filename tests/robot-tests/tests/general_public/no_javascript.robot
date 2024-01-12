*** Settings ***
Resource        ../libs/public-common.robot
Library         ../libs/no_javascript.py
Resource        ../seed_data/seed_data_theme_1_constants.robot

Test Setup      fail test fast if required

Force Tags      GeneralPublic    Local    Dev    Test    Preprod    Prod


*** Test Cases ***
Parse Find Statistics page HTML
    [Documentation]    EES-1186
    # Search by keywords here so Seed Data Theme 1 Publication 1 is on the page.
    ${parsed_page}=    user gets parsed html from page
    ...    %{PUBLIC_URL}/find-statistics?sortBy=relevance&search=Pupil+absence+in+schools+in+England
    set suite variable    ${parsed_page}

Validate Seed Data Theme 1 Publication 1 publication on page
    ${list}=    user_gets_publications_list    ${parsed_page}
    user_checks_list_contains_publication    ${list}    ${SEED_DATA_THEME_1_PUBLICATION_1_TITLE}
