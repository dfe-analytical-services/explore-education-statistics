*** Settings ***
Resource    ../libs/common.robot

Force Tags  GeneralPublic   Local

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to Absence publication
    [Tags]  HappyPath
    environment variable should be set  PUBLIC_URL
    user goes to url  %{PUBLIC_URL}
    user waits until page contains  Select an option to find the national and regional

    user clicks link  Find statistics and data
    user waits until page contains  Browse to find the statistics and data you’re looking for

    user opens accordion section  Pupils and schools
    user opens details dropdown   Pupil absence
    user clicks element   css:[data-testid="view-stats-pupil-absence-in-schools-in-england"]
    user waits until page contains element   xpath://h1[text()="Pupil absence in schools in England"]
    user checks url contains  %{PUBLIC_URL}/find-statistics/pupil-absence-in-schools-in-england

Go to Notify me page for Absence publication
    [Tags]  HappyPath
    user clicks link  Sign up for email alerts

    user waits until page contains element  xpath://h1/span[text()="Notify me"]
    user waits until page contains element  xpath://h1[text()="Pupil absence in schools in England"]

    user checks element count is x      css:[data-testid="breadcrumbs--list"] li     4
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Find statistics and data
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(3)   Pupil absence in schools in England
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(4)   Notify me

Sign up for email alerts
    [Tags]  HappyPath
    sleep  100000
    user clicks element  css:#email-id
    user presses keys    mark+test@hiveit.co.uk
    user clicks element  css:#submit-button

Get temporary email address
    [Tags]  UnderConstruction
    ${S_ID}   ${EMAIL_ADD}    get email address
    set suite variable  ${SESSION_ID}   ${S_ID}
    set suite variable  ${EMAIL}    ${EMAIL_ADD}

Go to Absence publication page
    [Tags]  UnderConstructions
    environment variable should be set  PUBLIC_URL
    user goes to url  %{PUBLIC_URL}/find-statistics/pupil-absence-in-schools-in-england
    user checks element contains  css:body   Pupil absence data and statistics for schools in England

Request subscription to Absence publication
    [Tags]  UnderConstructions
    user clicks link   Notify me
    user waits until page contains element  css:#email-address
#    press keys   css:#email-id   ${EMAIL}
    press keys   css:#email-id   mark@hiveit.co.uk
    user clicks element    css:#submit-button
    user waits until page contains   Thank you. Please check your email to verify the subscription.

Visit confirmation link from email
    [Tags]   UnderConstruction
    ${EMAIL_BODY}  wait for email  ${SESSION_ID}   explore.education.statistics.test@notifications.service.gov.uk
    should contain   ${EMAIL_BODY}    You have requested that you wish to be notified when a new publication of Pupil absence in schools in England becomes available.
    ${CONFIRMATION_LINK}   get confirmation link   ${EMAIL_BODY}
    should contain    %{PUBLIC_URL}
    user goes to url    ${CONFIRMATION_LINK}
    user waits until page contains   Pupil absence in schools in England
    user waits until page contains   You have successfully subscribed to this publication.


Send dummy release notification
    environment varaible should be set   ADMIN_URL
    user goes to url    %{ADMIN_URL}/release/notify
    # TODO: Send dummy notification

Check publication notification email is received
    ${EMAIL_BODY}  wait for email  ${SESSION_ID}   explore.education.statistics.test@notifications.service.gov.uk
    should contain   ${EMAIL_BODY}    A new publication of Pupil absence in England is available.
    # TODO: Verify publication link and unsubscribe link



