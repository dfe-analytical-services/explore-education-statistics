*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Verify Public Page Loads
    [Tags]  HappyPath
    environment variable should be set  PUBLIC_URL
    user goes to url  %{PUBLIC_URL}
    user checks element contains  css:body   Explore education statistics

Verify can accept cookie banner
    [Tags]  HappyPath   NotAgainstLocal
    user checks page contains  GOV.UK uses cookies to make the site simpler.

    cookie should not exist   ees_banner_seen
    cookie should not exist   ees_disable_google_analytics

    user clicks element  xpath://button[text()="Accept Cookies"]

    cookie should have value  ees_banner_seen   true
    cookie should have value  ees_disable_google_analytics   false

Validate homepage
    [Tags]  HappyPath
    user checks page contains element  link:Find statistics and data
    user checks page contains element  link:Create your own tables online
    user checks page contains element  link:Download data files

    user checks page contains element  xpath://h3[text()="Supporting information"]
    user checks page contains element  link:Education statistics: methodology
    user checks page contains element  link:Education statistics: glossary

    user checks page contains element  xpath://h3[text()="Related services"]
    user checks page contains element  link:Find and compare schools in England
    user checks page contains element  link:Get information about schools
    user checks page contains element  link:Schools financial benchmarking

    user checks page contains element  xpath://h3[text()="Contact Us"]
    user checks page contains link with text and url  explore.statistics@education.gov.uk   mailto:explore.statistics@education.gov.uk

    user checks element count is x      css:[data-testid="breadcrumbs--list"] li     1
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home

    user checks page contains link with text and url   Open Government Licence v3.0  https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/
    user checks page contains link with text and url   © Crown copyright   https://www.nationalarchives.gov.uk/information-management/re-using-public-sector-information/uk-government-licensing-framework/crown-copyright/

Validate Cookies page
    [Tags]  HappyPath
    user clicks link   Cookies
    user waits until page contains heading   Cookies on Explore education statistics
    user checks url contains   %{PUBLIC_URL}/cookies

    user checks element count is x  css:[data-testid="breadcrumbs--list"] li     2
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Cookies

Disable google analytics
    [Tags]  HappyPath   NotAgainstLocal
    user clicks element   css:#googleAnalytics-off
    user clicks element   xpath://button[text()="Save changes"]
    user waits until page contains   Your cookie settings were saved

    cookie should have value  ees_banner_seen   true
    cookie should have value  ees_disable_google_analytics   true

Enable google analytics
    [Tags]  HappyPath    NotAgainstLocal
    user reloads page

    user clicks element   css:#googleAnalytics-on
    user clicks element   xpath://button[text()="Save changes"]
    user waits until page contains   Your cookie settings were saved

    cookie should have value  ees_banner_seen   true
    cookie should have value  ees_disable_google_analytics   false

Validate Cookies Details page
    [Tags]  HappyPath
    user clicks link    Find out more about cookies on Explore education statistics
    user waits until page contains heading   Details about cookies
    user checks url contains   %{PUBLIC_URL}/cookies/details

    user checks element count is x  css:[data-testid="breadcrumbs--list"] li     3
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Cookies
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(3)   Details about cookies

    cookie names should be on page

Validate Privacy notice page
    [Tags]  HappyPath
    user clicks link   Privacy notice
    user waits until page contains heading  Privacy notice
    user waits until page contains  The explore education statistics service is operated by the Department for Education

    user checks url contains  %{PUBLIC_URL}/privacy-notice

    user checks element count is x  css:[data-testid="breadcrumbs--list"] li     2
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Privacy notice

Validate Contact page
    [Tags]  HappyPath
    user clicks link    Contact us
    user waits until page contains  Contact explore education statistics
    user waits until page contains  General enquiries
    user waits until page contains  explore.statistics@education.gov.uk
    user waits until page contains  DfE Head of Profession for Statistics
    user waits until page contains  hop.statistics@education.gov.uk

    user checks url contains    %{PUBLIC_URL}/contact-us

    user checks element count is x  css:[data-testid="breadcrumbs--list"] li     2
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Contact

Validate Help and support page
    [Tags]  HappyPath
    user clicks link    Help and support
    user waits until page contains heading  Help and support

    user checks url contains    %{PUBLIC_URL}/help-support

    user checks element count is x  css:[data-testid="breadcrumbs--list"] li     2
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Help and support

Validate Sitemap page
    [Tags]  HappyPath
    user clicks link    Sitemap
    user waits until page contains heading  Sitemap

    user checks url contains    %{PUBLIC_URL}/sitemap

    user checks element count is x  css:[data-testid="breadcrumbs--list"] li     2
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Sitemap

Validate Feedback page
    [Tags]  HappyPath
    user clicks link  feedback
    user selects newly opened window
    user waits until page contains      Explore Education Statistics - Beta banner feedback survey

    user checks url contains    www.smartsurvey.co.uk
