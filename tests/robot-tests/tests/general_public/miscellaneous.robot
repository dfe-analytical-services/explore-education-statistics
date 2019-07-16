*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Verify Public Page Loads
    [Tags]  HappyPath
    user goes to url  ${url}
    user checks element contains  css:body   Explore education statistics

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
    user waits until page contains element   xpath://h1[text()="Cookies"]
    user waits until page contains  How cookies are used on this service

    cookie names should be on page

    user checks url contains   ${url}/cookies

    user checks element count is x  css:[data-testid="breadcrumbs--list"] li     2
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Cookies

Validate Privacy notice page
    [Tags]  HappyPath
    user clicks link   Privacy notice
    user waits until page contains element  xpath://h1[text()="Privacy notice"]
    user waits until page contains  The explore education statistics service is operated by the Department for Education

    user checks url contains  ${url}/privacy-notice

    user checks element count is x  css:[data-testid="breadcrumbs--list"] li     2
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Privacy notice

Validate Contact page
    [Tags]  HappyPath
    user clicks link    Contact us
    user waits until page contains  Contact explore education statistics
    user waits until page contains  If you need help and support or have a question about education statistics

    user checks url contains    ${url}/contact

    user checks element count is x  css:[data-testid="breadcrumbs--list"] li     2
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Contact

Validate Help and support page
    [Tags]  HappyPath
    user clicks link    Help and support
    user waits until page contains element  xpath:.//h1[text()="Help and support"]

    user checks url contains    ${url}/help-support

    user checks element count is x  css:[data-testid="breadcrumbs--list"] li     2
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Help and support

Validate Sitemap page
    [Tags]  HappyPath
    user clicks link    Sitemap
    user waits until page contains element  xpath:.//h1[text()="Sitemap"]

    user checks url contains    ${url}/sitemap

    user checks element count is x  css:[data-testid="breadcrumbs--list"] li     2
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(1)   Home
    user checks element should contain  css:[data-testid="breadcrumbs--list"] li:nth-child(2)   Sitemap

Validate Feedback page
    [Tags]  HappyPath
    user clicks link  feedback
    user waits until page contains      Explore Education Statistics - Beta banner feedback survey

    user checks url contains    www.smartsurvey.co.uk
