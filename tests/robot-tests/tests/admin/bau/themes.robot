*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../seed_data/seed_data_theme_1_constants.robot

Suite Setup         user signs in as bau1
Suite Teardown      teardown suite
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${THEME_NAME}               UI test theme - suite %{RUN_IDENTIFIER}

${CREATED_THEME_ID}         ${EMPTY}
${CREATED_THEME_NAME}       UI test theme - suite created %{RUN_IDENTIFIER}


*** Test Cases ***
Go to 'Manage themes'
    user clicks link    manage themes
    user waits until h1 is visible    Manage themes

Verify existing theme
    user waits until page contains accordion section    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    ${accordion}=    user opens accordion section    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user checks summary list contains    Summary
    ...    Including absence, application and offers, capacity exclusion and special educational needs (SEN) statistics
    ...    ${accordion}

Create theme
    user clicks link    Create theme
    user waits until h1 is visible    Create theme
    user enters text into element    id:themeForm-title    ${CREATED_THEME_NAME}
    user enters text into element    id:themeForm-summary    Created summary
    user clicks button    Save theme

Verify created theme
    user waits until h1 is visible    Manage themes
    user waits until page contains accordion section    ${CREATED_THEME_NAME}
    user checks testid element contains    Summary for ${CREATED_THEME_NAME}    Created summary

Edit theme
    user clicks element    testid:Edit link for ${CREATED_THEME_NAME}
    user waits until h1 is visible    Edit theme

    # Used in teardown
    ${theme_id}=    get theme id from url
    set suite variable    ${CREATED_THEME_ID}    ${theme_id}

    user waits until page contains element    id:themeForm-title
    user checks input field contains    id:themeForm-title    ${CREATED_THEME_NAME}
    user checks input field contains    id:themeForm-summary    Created summary
    user enters text into element    id:themeForm-title    ${THEME_NAME}
    user enters text into element    id:themeForm-summary    Updated summary
    user clicks button    Save theme
    user waits until h1 is visible    Manage themes

Verify updated theme
    user waits until page contains accordion section    ${THEME_NAME}
    user checks testid element contains    Summary for ${THEME_NAME}    Updated summary


*** Keywords ***
teardown suite
    IF    "${CREATED_THEME_ID}" != ""
        user deletes theme via api    ${CREATED_THEME_ID}
    END
    user closes the browser
