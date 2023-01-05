*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      teardown suite
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${THEME_NAME}               UI test theme - suite %{RUN_IDENTIFIER}
${TOPIC_NAME}               UI test topic - suite %{RUN_IDENTIFIER}

${CREATED_THEME_ID}         ${EMPTY}
${CREATED_THEME_NAME}       UI test theme - suite created %{RUN_IDENTIFIER}
${CREATED_TOPIC_NAME}       UI test topic - suite created %{RUN_IDENTIFIER}


*** Test Cases ***
Go to 'Manage themes and topics'
    user clicks link    manage themes and topics
    user waits until h1 is visible    Manage themes and topics

Verify existing theme and topics
    user waits until page contains accordion section    Pupils and schools
    user opens accordion section    Pupils and schools
    user checks summary list contains    Summary
    ...    Including absence, application and offers, capacity exclusion and special educational needs (SEN) statistics
    user checks topic is in correct position    Pupils and schools    1
    ...    Exclusions
    user checks topic is in correct position    Pupils and schools    2
    ...    Pupil absence
    user checks topic is in correct position    Pupils and schools    3
    ...    School and pupil numbers
    user checks topic is in correct position    Pupils and schools    4
    ...    School applications

Create theme
    user clicks link    Create theme
    user waits until h1 is visible    Create theme
    user enters text into element    id:themeForm-title    ${CREATED_THEME_NAME}
    user enters text into element    id:themeForm-summary    Created summary
    user clicks button    Save theme

Verify created theme
    user waits until h1 is visible    Manage themes and topics
    user waits until page contains accordion section    ${CREATED_THEME_NAME}
    user checks testid element contains    Summary for ${CREATED_THEME_NAME}    Created summary
    user checks element is visible
    ...    xpath://*[@data-testid="Topics for ${CREATED_THEME_NAME}"]//*[text()="No topics for this theme"]

Edit theme
    user clicks element    testid:Edit link for ${CREATED_THEME_NAME}
    user waits until h1 is visible    Edit theme

    # Used in teardown
    ${theme_id}    get theme id from url
    set suite variable    ${CREATED_THEME_ID}    ${theme_id}

    user waits until page contains element    id:themeForm-title
    user checks input field contains    id:themeForm-title    ${CREATED_THEME_NAME}
    user checks input field contains    id:themeForm-summary    Created summary
    user enters text into element    id:themeForm-title    ${THEME_NAME}
    user enters text into element    id:themeForm-summary    Updated summary
    user clicks button    Save theme
    user waits until h1 is visible    Manage themes and topics

Verify updated theme
    user waits until page contains accordion section    ${THEME_NAME}
    user checks testid element contains    Summary for ${THEME_NAME}    Updated summary
    user checks element is visible
    ...    xpath://*[@data-testid="Topics for ${THEME_NAME}"]//*[text()="No topics for this theme"]

Create topic
    user clicks element    testid:Create topic link for ${THEME_NAME}
    user waits until page contains title caption    ${THEME_NAME}
    user waits until h1 is visible    Create topic
    user enters text into element    id:topicForm-title    ${CREATED_TOPIC_NAME}
    user clicks button    Save topic

Verify created topic
    user waits until h1 is visible    Manage themes and topics
    user waits until page contains accordion section    ${THEME_NAME}
    user checks topic is in correct position    ${THEME_NAME}    1    ${CREATED_TOPIC_NAME}

Edit topic
    user clicks element    testid:Edit ${CREATED_TOPIC_NAME} topic link for ${THEME_NAME}
    user waits until page contains title caption    ${THEME_NAME}
    user waits until h1 is visible    Edit topic
    user waits until page contains element    id:topicForm-title
    user checks input field contains    id:topicForm-title    ${CREATED_TOPIC_NAME}
    user enters text into element    id:topicForm-title    ${TOPIC_NAME}
    user clicks button    Save topic

Verify updated topic
    user waits until h1 is visible    Manage themes and topics
    user waits until page contains accordion section    ${THEME_NAME}
    user checks topic is in correct position    ${THEME_NAME}    1    ${TOPIC_NAME}


*** Keywords ***
user checks topic is in correct position
    [Arguments]    ${theme}    ${position}    ${topic}
    user checks element should contain    css:[data-testid="Topics for ${theme}"] dl > div:nth-child(${position}) > dt
    ...    ${topic}

teardown suite
    IF    "${CREATED_THEME_ID}" != ""
        user deletes theme via api    ${CREATED_THEME_ID}
    END
    user closes the browser
