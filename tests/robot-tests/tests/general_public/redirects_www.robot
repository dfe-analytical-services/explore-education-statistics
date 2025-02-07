*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local


*** Test Cases ***
Verify that routes with www are redirected without them
    user navigates to www    %{PUBLIC_URL}/
    user waits until page contains    Explore education statistics
    user checks url equals    %{PUBLIC_URL}/

    user navigates to www    %{PUBLIC_URL}/data-catalogue/
    user waits until page contains    Data catalogue
    user checks url equals    %{PUBLIC_URL}/data-catalogue
