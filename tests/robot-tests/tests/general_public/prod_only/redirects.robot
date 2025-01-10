*** Settings ***
Resource            ../../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Prod


*** Test Cases ***
Verify that absolute paths with trailing slashes are redirected without them
    user navigates to public frontend    %{PUBLIC_URL}/data-catalogue/
    user waits until page contains    Data catalogue
    user checks url equals    %{PUBLIC_URL}/data-catalogue

    user navigates to public frontend    %{PUBLIC_URL}/glossary/?someRandomUrlParameter=123
    user waits until page contains    Glossary
    user checks url equals    %{PUBLIC_URL}/glossary?someRandomUrlParameter=123

Verify that redirects do not affect browser history
    user navigates to    about:blank

    user navigates to public frontend    %{PUBLIC_URL}/data-catalogue/
    user waits until page contains    Data catalogue
    user checks url equals    %{PUBLIC_URL}/data-catalogue

    user goes back
    user checks url equals    about:blank

Verify that routes without an absolute path still permit trailing slashes
    user navigates to public frontend    %{PUBLIC_URL}/
    user waits until page contains    Explore education statistics
    user checks url equals    %{PUBLIC_URL}/

    user navigates to public frontend    %{PUBLIC_URL}
    user waits until page contains    Explore education statistics
    user checks url equals    %{PUBLIC_URL}/

Verify that routes with www are redirected without them
    user navigates to public frontend with www    %{PUBLIC_URL}/
    user waits until page contains    Explore education statistics
    user checks url equals    %{PUBLIC_URL}/

    user navigates to public frontend with www    %{PUBLIC_URL}/data-catalogue/
    user waits until page contains    Data catalogue
    user checks url equals    %{PUBLIC_URL}/data-catalogue

Verify that routes with /1000 are redirected without them
    user navigates to public frontend    %{PUBLIC_URL}/data-catalogue/1000
    user waits until page contains    Data catalogue
    user checks url equals    %{PUBLIC_URL}/data-catalogue

Verify that routes with search parameters retain them
    user navigates to public frontend    %{PUBLIC_URL}/data-catalogue?foo=bar&baz=zod
    user waits until page contains    Data catalogue
    user checks url equals    %{PUBLIC_URL}/data-catalogue?foo=bar&baz=zod

Verify that multiple rules work together
    user navigates to public frontend    %{PUBLIC_URL}/data-catalogue/1000?foo=bar&baz=zod
    user waits until page contains    Data catalogue
    user checks url equals    %{PUBLIC_URL}/data-catalogue?foo=bar&baz=zod

    user navigates to public frontend    %{PUBLIC_URL}/data-catalogue/1000/?foo=bar
    user waits until page contains    Data catalogue
    user checks url equals    %{PUBLIC_URL}/data-catalogue?foo=bar
