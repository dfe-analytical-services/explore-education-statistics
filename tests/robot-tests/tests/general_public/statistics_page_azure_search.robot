*** Settings ***
Resource            ../libs/public-common.robot
Resource            ../seed_data/seed_data_theme_1_constants.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Dev    PreProd    Prod


*** Test Cases ***
Navigate to Find Statistics page with azure search enabled
    environment variable should be set    PUBLIC_URL
    user navigates to    %{PUBLIC_URL}/find-statistics?azsearch=true
    user waits until h1 is visible    Find statistics and data

Validate Related information section and links does not exist
    user checks page does not contain element    css:[aria-labelledby="related-information"]

Validate themes filters exist
    user checks select contains option    id:filters-form-theme    All themes
    user checks selected option label    id:filters-form-theme    All themes

Validate release type filters exist
    user checks select contains option    id:filters-form-release-type    All release types
    user checks selected option label    id:filters-form-release-type    All release types

Validate sort controls exist
    user checks select contains option    id:filters-form-sortBy    Newest
    user checks select contains option    id:filters-form-sortBy    Oldest
    user checks select contains option    id:filters-form-sortBy    A to Z

Validate publications list exists and all publications are shown
    user checks page contains element    testid:publicationsList
    user checks page contains element    xpath://p[contains(text(),"showing all publications")]

Search publications
    user clicks element    id:searchForm-search
    user presses keys    pupil absence
    user clicks button    Search
    user checks page contains button    pupil absence
    user checks page does not contain element    css:[class="govuk-warning-text"]
    user checks page does not contain element    xpath://p[contains(text(),"showing all publications")]

Validate search is reset by clearing search term
    user clicks button    pupil absence
    user waits until page does not contain button    pupil absence
    user checks page contains element    xpath://p[contains(text(),"showing all publications")]
