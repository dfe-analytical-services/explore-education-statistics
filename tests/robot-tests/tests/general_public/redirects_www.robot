*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local


*** Test Cases ***
Verify that routes with www are redirected without them
    user navigates to public frontend with www    %{PUBLIC_URL}/
    user waits until page contains    Explore education statistics
    user checks url equals    %{PUBLIC_URL}/

    user navigates to public frontend    %{PUBLIC_URL_WITH}/data-catalogue/
    user waits until page contains    Browse our open data
    user checks url equals    %{PUBLIC_URL}/data-catalogue
