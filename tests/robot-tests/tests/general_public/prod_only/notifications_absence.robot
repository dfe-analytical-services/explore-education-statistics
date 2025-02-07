*** Settings ***
Resource            ../../libs/common.robot

Force Tags          GeneralPublic    Prod

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
Navigate to Absence publication
    user navigates to    %{PUBLIC_URL}/find-statistics/pupil-absence-in-schools-in-england
    user waits until h1 is visible    Pupil absence in schools in England    %{WAIT_MEDIUM}

Go to Notify me page for Absence publication
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
    [Tags]    Failing    # Currently failing on Production when running UI tests directly following deploys.
    user clicks element    id:subscriptionForm-email
    press keys    id:subscriptionForm-email    mark@hiveit.co.uk
    user clicks button    Subscribe

    # EES-1265
    user waits until page contains    Subscribed    %{WAIT_LONG}
    user checks page contains    Thank you. Check your email to confirm your subscription.
