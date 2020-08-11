*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        UI test topic %{RUN_IDENTIFIER}
${PUBLICATION_NAME}  UI tests - themes and topics %{RUN_IDENTIFIER}

*** Test Cases ***
Go to 'Manage themes and topics'
    [Tags]  HappyPath
    environment variable should be set   RUN_IDENTIFIER
    user clicks link  manage themes and topics
    user waits until page contains heading 1  Manage themes and topics

Verify existing theme and topic
   [Tags]  HappyPath
   user waits until page contains accordion section  Children, early years and social care
   user clicks button   Children, early years and social care

#Create theme
#    [Tags]  HappyPath
#    user waits until page contains heading 1  Create theme
#    user enters text into element  id:themeForm-title   ${title}
#    user enters text into element  id:themeForm-summary  ${summary}
#    user clicks button  Save theme
#    user waits until page contains heading 1  Manage themes and topics
#    user waits until page contains accordion section  ${title}
#    user clicks link  Home
#    user waits until page contains heading 1  Dashboard
#
