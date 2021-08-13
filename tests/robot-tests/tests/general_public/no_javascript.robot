*** Settings ***
Resource        ../libs/public-common.robot
Library         ../libs/no_javascript.py

Force Tags      GeneralPublic    Local    Dev    Test    Preprod    Prod

*** Test Cases ***
Parse Find Statistics page HTML
    [Documentation]    EES-1186
    ${parsed_page}=    user gets parsed html from page    %{PUBLIC_URL}/find-statistics
    set suite variable    ${parsed_page}

Validate Pupils and schools accordion section on page
    ${accordion}=    user gets page accordion section    ${parsed_page}    Pupils and schools
    ${accordion_heading}=    user gets accordion heading    ${accordion}
    user checks accordion heading is not tag type    ${accordion_heading}    button
    user checks accordion heading does not have attribute aria expanded    ${accordion_heading}

    ${details}=    user gets accordion section details    ${accordion}    Pupil absence
    user checks details summary does not have attribute aria expanded    ${details}
    user checks details contains content    ${details}    Pupil absence in schools in England
