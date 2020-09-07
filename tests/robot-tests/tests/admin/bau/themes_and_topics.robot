*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${THEME_NAME}           UI test theme - suite %{RUN_IDENTIFIER}
${THEME_NAME_CREATED}   UI test theme - suite created %{RUN_IDENTIFIER}
${TOPIC_NAME}           UI test topic - suite %{RUN_IDENTIFIER}
${TOPIC_NAME_CREATED}   UI test topic - suite created %{RUN_IDENTIFIER}

*** Keywords ***
user checks topic is in correct position
    [Arguments]  ${theme}  ${position}  ${topic}
    user checks element should contain  css:[data-testid="Topics for ${theme}"] dl > div:nth-child(${position}) > dt  ${topic}

*** Test Cases ***
Go to 'Manage themes and topics'
    [Tags]  HappyPath
    user clicks link  manage themes and topics
    user waits until h1 is visible  Manage themes and topics

Verify existing theme and topics
    [Tags]  HappyPath
    user waits until page contains accordion section  Children, early years and social care
    user opens accordion section  Children, early years and social care
    user checks summary list contains  Summary    Including children in need, EYFS, and looked after children and social workforce statistics
    user checks topic is in correct position  Children, early years and social care  1  Childcare and early years
    user checks topic is in correct position  Children, early years and social care  2  Children in need and child protection
    user checks topic is in correct position  Children, early years and social care  3  Children's social work workforce
    user checks topic is in correct position  Children, early years and social care  4  Early years foundation stage profile
    user checks topic is in correct position  Children, early years and social care  5  Looked-after children
    user checks topic is in correct position  Children, early years and social care  6  Secure children's homes

Create theme
    [Tags]  HappyPath
    user clicks link    Create theme
    user waits until h1 is visible  Create theme
    user enters text into element  id:themeForm-title   ${THEME_NAME_CREATED}
    user enters text into element  id:themeForm-summary  Created summary
    user clicks button  Save theme

Verify created theme
    [Tags]  HappyPath
    user waits until h1 is visible  Manage themes and topics
    user waits until page contains accordion section  ${THEME_NAME_CREATED}
    user checks testid element contains  Summary for ${THEME_NAME_CREATED}  Created summary
    user checks element is visible  xpath://*[@data-testid="Topics for ${THEME_NAME_CREATED}"]//*[text()="No topics for this theme"]

Edit theme
    [Tags]  HappyPath
    user clicks testid element  Edit link for ${THEME_NAME_CREATED}
    user waits until h1 is visible  Edit theme
    user waits until page contains element   id:themeForm-title
    user checks input field contains  id:themeForm-title   ${THEME_NAME_CREATED}
    user checks input field contains  id:themeForm-summary   Created summary
    user enters text into element  id:themeForm-title   ${THEME_NAME}
    user enters text into element  id:themeForm-summary  Updated summary
    user clicks button  Save theme
    user waits until h1 is visible  Manage themes and topics

Verify updated theme
    [Tags]  HappyPath
    user waits until page contains accordion section  ${THEME_NAME}
    user checks testid element contains  Summary for ${THEME_NAME}  Updated summary
    user checks element is visible  xpath://*[@data-testid="Topics for ${THEME_NAME}"]//*[text()="No topics for this theme"]

Create topic
    [Tags]  HappyPath
    user clicks testid element  Create topic link for ${THEME_NAME}
    user waits until page contains title caption  ${THEME_NAME}
    user waits until h1 is visible  Create topic
    user enters text into element  id:topicForm-title   ${TOPIC_NAME_CREATED}
    user clicks button  Save topic

Verify created topic
    [Tags]  HappyPath
    user waits until h1 is visible  Manage themes and topics
    user waits until page contains accordion section  ${THEME_NAME}
    user checks topic is in correct position  ${THEME_NAME}  1  ${TOPIC_NAME_CREATED}

Edit topic
    [Tags]  HappyPath
    user clicks testid element  Edit ${TOPIC_NAME_CREATED} topic link for ${THEME_NAME}
    user waits until page contains title caption  ${THEME_NAME}
    user waits until h1 is visible  Edit topic
    user waits until page contains element   id:topicForm-title
    user checks input field contains  id:topicForm-title   ${TOPIC_NAME_CREATED}
    user enters text into element  id:topicForm-title   ${TOPIC_NAME}
    user clicks button  Save topic

Verify updated topic
    [Tags]  HappyPath
    user waits until h1 is visible  Manage themes and topics
    user waits until page contains accordion section  ${THEME_NAME}
    user checks topic is in correct position  ${THEME_NAME}  1  ${TOPIC_NAME}

Delete test theme
    [Tags]  HappyPath
    user clicks testid element  Edit link for ${THEME_NAME}
    user waits until h1 is visible  Edit theme
    ${theme_id}=  get theme id from url
    delete theme  ${theme_id}
    user closes the browser
