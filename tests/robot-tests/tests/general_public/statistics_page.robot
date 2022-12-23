*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Prod


*** Test Cases ***
Navigate to Find Statistics page
    [Tags]    Local    Dev
    environment variable should be set    PUBLIC_URL
    user navigates to find statistics page on public frontend

check bootstrapped data
    [Tags]    Local    Dev    NotAgainstProd
    user waits until page contains accordion section    Children, early years and social care
    user waits until page contains accordion section    Destination of pupils and students
    user waits until page contains accordion section    Finance and funding
    user waits until page contains accordion section    Further education
    user waits until page contains accordion section    Higher education
    user waits until page contains accordion section    Pupils and schools
    user waits until page contains accordion section    School and college outcomes and performance
    user waits until page contains accordion section    Teachers and school workforce
    user waits until page contains accordion section    UK education and training statistics
