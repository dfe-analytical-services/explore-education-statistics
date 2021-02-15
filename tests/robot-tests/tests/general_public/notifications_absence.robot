*** Settings ***
Resource    ../libs/common.robot

Force Tags  GeneralPublic  Dev  Test  Preprod  Prod

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Navigate to Absence publication
    [Tags]  HappyPath  Local
    environment variable should be set  PUBLIC_URL
    user goes to url  %{PUBLIC_URL}
    user waits until page contains  Select an option to find the national and regional

    user clicks link  Explore
    user waits until page contains  Browse to find the statistics and data you’re looking for
    user waits for page to finish loading

    user opens accordion section  Pupils and schools
    user opens details dropdown   Pupil absence
    user clicks testid element  View stats link for Pupil absence in schools in England
    user waits until h1 is visible  Pupil absence in schools in England
    user checks url contains  %{PUBLIC_URL}/find-statistics/pupil-absence-in-schools-in-england

Go to Notify me page for Absence publication
    [Tags]  HappyPath  Local
    user clicks link  Sign up for email alerts

    user waits until page contains title caption  Notify me     180
    user waits until h1 is visible  Pupil absence in schools in England

    user checks breadcrumb count should be  4
    user checks nth breadcrumb contains  1   Home
    user checks nth breadcrumb contains  2   Find statistics and data
    user checks nth breadcrumb contains  3   Pupil absence in schools in England
    user checks nth breadcrumb contains  4   Notify me

Sign up for email alerts
    [Tags]  HappyPath   NotAgainstPreProd
    [Documentation]   EES-716  EES-1265
    user clicks element  id:subscriptionForm-email
    press keys  id:subscriptionForm-email  mark@hiveit.co.uk
    user clicks button   Subscribe

    # EES-1265
    user waits until h1 is visible   Subscribed     180
    user checks page contains  Thank you. Check your email to confirm your subscription.
