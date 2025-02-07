*** Settings ***
Resource            ../libs/common.robot
Resource            ../seed_data/seed_data_theme_1_constants.robot

Force Tags          GeneralPublic    Local    Dev    Test    Preprod

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Test Cases ***
Navigate to Absence publication
    user navigates to    %{PUBLIC_URL}${PUPIL_ABSENCE_PUBLICATION_RELATIVE_URL}
    user waits until h1 is visible    ${PUPIL_ABSENCE_PUBLICATION_TITLE}    %{WAIT_MEDIUM}

Go to Notify me page for Absence publication
    user clicks link    Sign up for email alerts

    user waits until page contains title caption    Notify me    %{WAIT_LONG}
    user waits until h1 is visible    ${PUPIL_ABSENCE_PUBLICATION_TITLE}

    user checks breadcrumb count should be    4
    user checks nth breadcrumb contains    1    Home
    user checks nth breadcrumb contains    2    Find statistics and data
    user checks nth breadcrumb contains    3    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user checks nth breadcrumb contains    4    Notify me

Sign up for email alerts
    [Tags]    NotAgainstLocal
    user clicks element    id:subscriptionForm-email
    press keys    id:subscriptionForm-email    mark@hiveit.co.uk
    user clicks button    Subscribe

    # EES-1265
    user waits until page contains    Subscribed    %{WAIT_LONG}
    user checks page contains    Thank you. Check your email to confirm your subscription.
