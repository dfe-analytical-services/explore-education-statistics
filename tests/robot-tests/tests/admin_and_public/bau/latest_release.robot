*** Settings ***
Resource    ../../libs/admin-common.robot
Resource    ../../libs/charts.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${RUN_IDENTIFIER}    %{RUN_IDENTIFIER}
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - latest release %{RUN_IDENTIFIER}

*** Test Cases ***
Create publication for "UI tests topic" topic
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains link    Create new publication
    user checks page does not contain button   ${PUBLICATION_NAME}
    user clicks link  Create new publication
    user creates publication    ${PUBLICATION_NAME}

Verify publication
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains button  ${PUBLICATION_NAME}

Create release
    [Tags]  HappyPath
    user opens accordion section  ${PUBLICATION_NAME}
    user clicks element  css:[data-testid="Create new release link for ${PUBLICATION_NAME}"]
    user creates release for publication  ${PUBLICATION_NAME}  Calendar Year    2000

Go to "Release status" tab
    [Tags]  HappyPath
    user clicks link   Release status
    user waits until h2 is visible  Release status
    user waits until page contains button  Edit release status

Approve release
    [Tags]  HappyPath
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status

    user clicks element   css:input[data-testid="Approved for publication"]
    user enters text into element  id:releaseStatusForm-internalReleaseNote  Approved by UI tests - latest release
    user clicks element  css:input[data-testid="As soon as possible"]

    user clicks button   Update status

Verify release is scheduled
    [Tags]  HappyPath
    user waits until h2 is visible  Release status
    user checks summary list contains  Current status  Approved

Wait for release process status to be Complete
    [Tags]  HappyPath
    # EES-1007 - Release process status doesn't automatically update
    user waits for release process status to be  Complete    900
    user checks page does not contain button  Edit release status

Return to Admin Dashboard
    [Tags]  HappyPath
    user goes to url    %{ADMIN_URL}
    user waits until h1 is visible   Dashboard
    user waits until page contains element   css:#selectTheme    180

Create another release for the same publication
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user opens accordion section   ${PUBLICATION_NAME}
    user clicks element  css:[data-testid="Create new release link for ${PUBLICATION_NAME}"]
    user creates release for publication  ${PUBLICATION_NAME}  Calendar Year    2001

Approve new release
    [Tags]  HappyPath
    user clicks link   Release status
    user waits until h2 is visible  Release status
    user waits until page contains button  Edit release status
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status

    user clicks element   css:input[data-testid="Approved for publication"]
    user enters text into element  id:releaseStatusForm-internalReleaseNote  Approved by UI tests - latest release - part deux
    user clicks element  css:input[data-testid="As soon as possible"]

    user clicks button   Update status

Wait for new release to be Complete
    [Tags]  HappyPath
    user waits until h2 is visible  Release status
    user checks summary list contains  Current status  Approved

    # EES-1007 - Release process status doesn't automatically update
    user waits for release process status to be  Complete    900
    user checks page does not contain button  Edit release status

User goes to public Find Statistics page
    [Tags]  HappyPath
    environment variable should be set   PUBLIC_URL
    user goes to url   %{PUBLIC_URL}/find-statistics
    user waits until h1 is visible  Find statistics and data
    user waits for page to finish loading

Verify published release is on Find Statistics page
    [Tags]  HappyPath
    user waits until page contains accordion section   Test theme
    user opens accordion section  Test theme
    user waits until accordion section contains text   Test theme   ${TOPIC_NAME}

    user opens details dropdown  ${TOPIC_NAME}
    user waits until details dropdown contains publication    ${TOPIC_NAME}  ${PUBLICATION_NAME}   10
    user checks publication bullet contains link   ${PUBLICATION_NAME}  View statistics and data
    user checks publication bullet contains link   ${PUBLICATION_NAME}  Create your own tables online
    user checks publication bullet does not contain link  ${PUBLICATION_NAME}   Statistics at DfE

Navigate to published release page
    [Tags]  HappyPath
    user clicks testid element  View stats link for ${PUBLICATION_NAME}
    user waits until h1 is visible  ${PUBLICATION_NAME}  90

Verify 2001 release is displayed correctly
    [Tags]  HappyPath
    user checks testid element contains  page-title-caption    Calendar Year 2001
    user checks page contains   This is the latest data
    user checks page contains   See 1 other releases

    user opens details dropdown  See 1 other releases
    user checks page contains other release   Calendar Year 2000
    user checks page does not contain other release   Calendar Year 2001

    user clicks link   Calendar Year 2000

Verify 2000 release displayed correctly
    [Tags]  HappyPath
    user checks testid element contains   page-title-caption   Calendar Year 2000
    user waits until page contains link    View latest data: Calendar Year 2001
    user checks page contains   See 1 other releases
    user checks page does not contain other release   Calendar Year 2000
    user checks page contains other release    Calendar Year 2001
