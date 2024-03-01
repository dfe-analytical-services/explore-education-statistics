*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local    Dev    Test    Preprod    Prod


*** Test Cases ***
Verify that absolute paths with trailing slashes are redirected without them
    user navigates to public frontend    %{PUBLIC_URL}/data-catalogue/
    user waits until page contains    Browse our open data
    user checks url equals    %{PUBLIC_URL}/data-catalogue

    user navigates to public frontend    %{PUBLIC_URL}/data-catalogue
    user waits until page contains    Browse our open data
    user checks url equals    %{PUBLIC_URL}/data-catalogue

    user navigates to public frontend    %{PUBLIC_URL}/glossary/?someRandomUrlParameter=123
    user waits until page contains    Glossary
    user checks url equals    %{PUBLIC_URL}/glossary?someRandomUrlParameter=123

    # Would be amazing if we could verify these redirects were done with a 301 rather than a 308...

Verify that redirects do not affect browser history
    user navigates to    about:blank

    user navigates to public frontend    %{PUBLIC_URL}/data-catalogue/
    user waits until page contains    Browse our open data
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

Verify that meta-guidance is redirected to data-guidance
    user navigates to public frontend    %{PUBLIC_URL}/find-statistics/seed-publication-release-approver/meta-guidance
    user waits until page contains    Seed publication - Release Approver
    user checks url equals    %{PUBLIC_URL}/find-statistics/seed-publication-release-approver/data-guidance

    user navigates to public frontend    %{PUBLIC_URL}/find-statistics/seed-publication-release-approver/meta-guidance/
    user waits until page contains    Seed publication - Release Approver
    user checks url equals    %{PUBLIC_URL}/find-statistics/seed-publication-release-approver/data-guidance

Verify that download-latest-data is redirected to data-catalogue
    user navigates to public frontend    %{PUBLIC_URL}/download-latest-data
    user waits until page contains    Browse our open data
    user checks url equals    %{PUBLIC_URL}/data-catalogue

    user navigates to public frontend    %{PUBLIC_URL}/download-latest-data/
    user waits until page contains    Browse our open data
    user checks url equals    %{PUBLIC_URL}/data-catalogue
