*** Settings ***
Resource            ../libs/common.robot

Force Tags          GeneralPublic    Dev    Test    Preprod    Prod

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
Navigate to Absence publication
    [Tags]    Local
    user navigates to public frontend
    user waits until page contains    Explore our statistics and data

    user clicks link    Explore
    user waits until page contains
    ...    Browse to find the statistics and data you’re looking for and open the section to get links to:
    ...    %{WAIT_MEDIUM}
    user waits for page to finish loading

    user opens accordion section    Pupils and schools
    user opens details dropdown    Pupil absence
    user clicks element    testid:View stats link for Pupil absence in schools in England
    user waits until h1 is visible    Pupil absence in schools in England    %{WAIT_MEDIUM}
    user checks url contains    %{PUBLIC_URL}/find-statistics/pupil-absence-in-schools-in-england

Go to Notify me page for Absence publication
    [Tags]    Local
    user clicks link    Sign up for email alerts

    user waits until page contains title caption    Notify me    %{WAIT_LONG}
    user waits until h1 is visible    Pupil absence in schools in England

    user checks breadcrumb count should be    4
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Find statistics and data
    user checks nth breadcrumb contains    3    Pupil absence in schools in England
    user checks nth breadcrumb contains    4    Notify me

Sign up for email alerts
    [Documentation]    EES-716    EES-1265
    [Tags]    NotAgainstPreProd
    user clicks element    id:subscriptionForm-email
    press keys    id:subscriptionForm-email    mark@hiveit.co.uk
    user clicks button    Subscribe

    # EES-1265
    user waits until h1 is visible    Subscribed    %{WAIT_LONG}
    user checks page contains    Thank you. Check your email to confirm your subscription.
