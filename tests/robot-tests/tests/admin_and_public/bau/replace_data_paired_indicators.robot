*** Settings ***
Library             String
Resource            ../../libs/admin-common.robot
Resource            ../../libs/public-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/table_tool.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}             Replace data paired indicators %{RUN_IDENTIFIER}
${RELEASE_NAME}                 Calendar year 2022
${SUBJECT_NAME}                 UI test subject
${PAIRED_INDICATORS_FORM}       id:pairedIndicatorsForm
${REPLACEMENT_PAIRS_TABLE}      testid:paired-indicators-table
${INDICATORS}                   xpath://*[@id="filtersForm-indicators"]
${ENROLMENTS_LABEL}             ${INDICATORS}//label[text()="Number of pupil enrolments"]
${OVERALL_SESSIONS_LABEL}       ${INDICATORS}//label[text()="Overall absence sessions"]
${OVERALL_RATE_CHECKBOX}        ${INDICATORS}//label[text()="Overall absence rate"]/../input[@type="checkbox"]
${AUTHORISED_RATE_CHECKBOX}     ${INDICATORS}//label[text()="Authorised absence rate"]/../input[@type="checkbox"]


*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    CY    2022

Upload subject to release
    user navigates to draft release page from dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

    user uploads subject and waits until complete
    ...    ${SUBJECT_NAME}
    ...    paired-indicators.csv
    ...    paired-indicators.meta.csv

Open manage indicators for the subject
    user clicks link    Manage filters and indicators
    user waits until h2 is visible    Manage filters and indicators
    user waits until table is visible    id:reordering
    user clicks button in table cell    1    2    Manage indicators    id:reordering
    user waits until h3 is visible    Manage indicators for ${SUBJECT_NAME}
    user waits until page contains    No indicators have been paired for this data set.

Pair each absence session count with its rate
    user clicks button    Edit paired indicators    id:reordering
    user waits until h3 is visible    Pair indicators for ${SUBJECT_NAME}

    user clicks button    Add pair    ${PAIRED_INDICATORS_FORM}
    user chooses select option    id:pairedIndicatorsForm-pairedIndicators-0-parentIndicatorId
    ...    Overall absence sessions
    user chooses select option    id:pairedIndicatorsForm-pairedIndicators-0-childIndicatorId
    ...    Overall absence rate

    user clicks button    Add another pair    ${PAIRED_INDICATORS_FORM}
    user chooses select option    id:pairedIndicatorsForm-pairedIndicators-1-parentIndicatorId
    ...    Authorised absence sessions
    user chooses select option    id:pairedIndicatorsForm-pairedIndicators-1-childIndicatorId
    ...    Authorised absence rate

    user clicks button    Add another pair    ${PAIRED_INDICATORS_FORM}
    user chooses select option    id:pairedIndicatorsForm-pairedIndicators-2-parentIndicatorId
    ...    Unauthorised absence sessions
    user chooses select option    id:pairedIndicatorsForm-pairedIndicators-2-childIndicatorId
    ...    Unauthorised absence rate

    user clicks button    Save paired indicators    ${PAIRED_INDICATORS_FORM}
    user waits until h3 is not visible    Pair indicators for ${SUBJECT_NAME}
    user waits until h3 is visible    Manage indicators for ${SUBJECT_NAME}

Check the saved pairs
    user checks table body has x rows    3    id:reordering
    user checks table cell contains    1    1    Overall absence sessions    id:reordering
    user checks table cell contains    1    2    Overall absence rate    id:reordering
    user checks table cell contains    2    1    Authorised absence sessions    id:reordering
    user checks table cell contains    2    2    Authorised absence rate    id:reordering
    user checks table cell contains    3    1    Unauthorised absence sessions    id:reordering
    user checks table cell contains    3    2    Unauthorised absence rate    id:reordering

Check a paired indicator is selected with its main indicator in the table tool
    user starts creating a data block up to choosing indicators

    # Smoke check that the saved pairs reach the table tool. The detailed behaviour is covered by Jest tests
    user checks element is disabled    ${OVERALL_RATE_CHECKBOX}
    user clicks indicator checkbox    Overall absence sessions
    user checks indicator checkbox is checked    Overall absence rate

    user clicks link    Data and files
    user waits until page contains data uploads table

Upload replacement that renames one rate and removes another
    user clicks link    Data uploads
    user waits until page contains data uploads table
    # The replacement renames sess_authorised_percent to sess_auth_percent and removes sess_unauthorised_percent
    user uploads subject replacement    ${SUBJECT_NAME}    paired-indicators-replacement.csv
    ...    paired-indicators-replacement.meta.csv
    user waits until page contains element    testid:Data file replacements table
    user confirms replacement upload    ${SUBJECT_NAME}

Open the data replacement page
    user clicks link in table cell    1    4    View details    testid:Data file replacements table
    user waits until page contains element    testid:Replacement Title
    user checks headed table body row cell contains    Data file import status    2    Complete
    ...    wait=%{WAIT_DATA_FILE_IMPORT}

Check paired indicators that will be maintained or dropped
    user waits until h3 is visible    Paired indicators    %{WAIT_MEDIUM}
    user waits until page contains
    ...    One or more paired indicators will be dropped by this data replacement
    user checks table body has x rows    3    ${REPLACEMENT_PAIRS_TABLE}

    user checks table cell contains    1    1    Overall absence sessions    ${REPLACEMENT_PAIRS_TABLE}
    user checks table cell contains    1    2    Overall absence rate    ${REPLACEMENT_PAIRS_TABLE}
    user checks table cell contains    1    3    Maintained    ${REPLACEMENT_PAIRS_TABLE}

    # The renamed paired indicator isn't mapped automatically, so the pair is dropped
    user checks table cell contains    2    1    Authorised absence sessions    ${REPLACEMENT_PAIRS_TABLE}
    user checks table cell contains    2    2    Authorised absence rate    ${REPLACEMENT_PAIRS_TABLE}
    user checks table cell contains    2    3    Dropped    ${REPLACEMENT_PAIRS_TABLE}

    # The removed paired indicator has nothing to map to, so the pair is dropped
    user checks table cell contains    3    1    Unauthorised absence sessions    ${REPLACEMENT_PAIRS_TABLE}
    user checks table cell contains    3    2    Unauthorised absence rate    ${REPLACEMENT_PAIRS_TABLE}
    user checks table cell contains    3    3    Dropped    ${REPLACEMENT_PAIRS_TABLE}

Follow the link to manage paired indicators
    user clicks link    Manage paired indicators
    user waits until h2 is visible    Manage filters and indicators
    user waits until h3 is visible    Manage indicators for ${SUBJECT_NAME}
    user checks table body has x rows    3    id:reordering
    user clicks button    Back to data files    id:reordering

Confirm data replacement
    # Confirming also shows that dropped pairs don't block the replacement
    user clicks link    Data uploads
    # The subject is being replaced, so only the replacements table is shown
    user waits until page contains data uploads replacements table
    user clicks link in table cell    1    4    View details    testid:Data file replacements table
    user waits until h3 is visible    Paired indicators    %{WAIT_MEDIUM}
    user clicks button    Confirm data replacement
    user waits until h2 is visible    Data replacement complete

Check the replacement complete page shows what happened to the paired indicators
    user waits until h3 is visible    Paired indicators
    user waits until page contains    The following paired indicators were maintained:
    user checks list has x items    testid:maintained-paired-indicators    1
    user checks list item contains    testid:maintained-paired-indicators    1
    ...    Overall absence sessions, paired with Overall absence rate

    user waits until page contains    The following paired indicators were dropped
    user checks list has x items    testid:dropped-paired-indicators    2
    user checks list item contains    testid:dropped-paired-indicators    1
    ...    Authorised absence sessions, paired with Authorised absence rate
    user checks list item contains    testid:dropped-paired-indicators    2
    ...    Unauthorised absence sessions, paired with Unauthorised absence rate

Check only the maintained pair remains after data replacement
    user clicks link    Manage paired indicators
    user waits until h2 is visible    Manage filters and indicators
    user waits until h3 is visible    Manage indicators for ${SUBJECT_NAME}

    user checks table body has x rows    1    id:reordering
    user checks table cell contains    1    1    Overall absence sessions    id:reordering
    user checks table cell contains    1    2    Overall absence rate    id:reordering

Check indicators from a dropped pair can be selected on their own in the table tool
    user starts creating a data block up to choosing indicators

    # The authorised rate was renamed and not mapped, so its pair was dropped
    user checks element is enabled    ${AUTHORISED_RATE_CHECKBOX}
    user clicks indicator checkbox    Authorised absence rate
    user checks indicator checkbox is checked    Authorised absence rate
    user checks indicator checkbox is not checked    Authorised absence sessions

    # The overall pair was maintained, so its paired indicator still can't be selected on its own
    user checks element is disabled    ${OVERALL_RATE_CHECKBOX}

Add data guidance to subject
    user navigates to Data Guidance page and adds data guidance for subject    ${SUBJECT_NAME}
    ...    ${SUBJECT_NAME} Main guidance content

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve release
    user approves release for immediate publication

Get public release link
    ${PUBLIC_RELEASE_LINK}    user gets url public release will be accessible at
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

Verify newly published release is public
    user navigates to public release page    ${PUBLIC_RELEASE_LINK}    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Collect the ids for table tool search links from the public table tool
    # Table tool search links pass ids without dashes, so read them from the public table tool's inputs
    user navigates to data tables page on public frontend
    environment variable should be set    TEST_THEME_NAME
    user clicks radio    %{TEST_THEME_NAME}
    user clicks radio    ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit

    user waits until table tool wizard step is available    2    Select a data set
    ${subject_id}    user gets search link id    xpath://label[text()="${SUBJECT_NAME}"]/../input[@type="radio"]
    user clicks radio    ${SUBJECT_NAME}
    user clicks element    id:publicationDataStepForm-submit

    user waits until table tool wizard step is available    3    Choose locations
    user clicks element    xpath://button[@data-testid="filter-accordion-button"][contains(., "National")]
    ${england_id}    user gets search link id    xpath://label[text()="England"]/../input[@type="checkbox"]
    user clicks checkbox    England
    user clicks element    id:locationFiltersForm-submit

    user waits until table tool wizard step is available    4    Choose time period
    user chooses select option    id:timePeriodForm-start    2021/22
    user chooses select option    id:timePeriodForm-end    2023/24
    ${start}    Get Selected List Value    id:timePeriodForm-start
    ${end}    Get Selected List Value    id:timePeriodForm-end
    ${time_period}    Replace String    ${start}|${end}    _    |
    user clicks element    id:timePeriodForm-submit

    user waits until table tool wizard step is available    5    Choose your filters
    user waits until page contains element    ${ENROLMENTS_LABEL}
    # Fail here, rather than in the search link checks, if the public site doesn't have the pairs
    user checks element is disabled    ${OVERALL_RATE_CHECKBOX}
    ${primary_id}    user gets search link id    xpath://label[text()="Primary"]/../input[@type="checkbox"]
    ${secondary_id}    user gets search link id    xpath://label[text()="Secondary"]/../input[@type="checkbox"]
    ${special_id}    user gets search link id    xpath://label[text()="Special"]/../input[@type="checkbox"]

    ${ENROLMENTS_ID}    user gets search link id    ${ENROLMENTS_LABEL}/../input[@type="checkbox"]
    ${OVERALL_SESSIONS_ID}    user gets search link id    ${OVERALL_SESSIONS_LABEL}/../input[@type="checkbox"]
    ${OVERALL_RATE_ID}    user gets search link id    ${OVERALL_RATE_CHECKBOX}
    ${AUTHORISED_RATE_ID}    user gets search link id    ${AUTHORISED_RATE_CHECKBOX}
    Set Suite Variable    ${ENROLMENTS_ID}
    Set Suite Variable    ${OVERALL_SESSIONS_ID}
    Set Suite Variable    ${OVERALL_RATE_ID}
    Set Suite Variable    ${AUTHORISED_RATE_ID}

    ${data_tables_link}    Replace String    ${PUBLIC_RELEASE_LINK}    /find-statistics/    /data-tables/
    Set Suite Variable
    ...    ${SEARCH_LINK}
    ...    ${data_tables_link}?fromSearch&sub=${subject_id}&tp=${time_period}&f=${primary_id},${secondary_id},${special_id}&loc=${england_id}

Check a search link for a main indicator also shows its paired indicator
    user opens table tool search link for indicators    ${OVERALL_SESSIONS_ID}

    user checks results table shows indicator    Overall absence sessions
    user checks results table shows indicator    Overall absence rate
    user checks results table contains value    6,954,347
    user checks results table contains value    5.91%

Check a search link keeps a paired indicator, an unrelated indicator and one from a dropped pair
    # The overall rate's main indicator is added so the rate is kept. The authorised pair was dropped by
    # the replacement, so its former main indicator isn't added.
    user opens table tool search link for indicators    ${OVERALL_RATE_ID},${ENROLMENTS_ID},${AUTHORISED_RATE_ID}

    user checks results table shows indicator    Overall absence sessions
    user checks results table shows indicator    Overall absence rate
    user checks results table shows indicator    Number of pupil enrolments
    user checks results table shows indicator    Authorised absence rate
    user checks results table contains value    6,954,347
    user checks results table contains value    5.91%
    user checks results table contains value    367,520
    user checks results table contains value    3.63%
    user checks page does not contain element    xpath://table[contains(., "Authorised absence sessions")]


*** Keywords ***
user gets search link id
    [Arguments]    ${locator}
    user waits until page contains element    ${locator}
    ${id}    Get Element Attribute    ${locator}    value
    ${id}    Remove String    ${id}    -
    [Return]    ${id}

user opens table tool search link for indicators
    [Arguments]    ${indicator_ids}
    user navigates to    ${SEARCH_LINK}&ind=${indicator_ids}
    user waits until results table appears    %{WAIT_LONG}

user checks results table shows indicator
    [Arguments]    ${label}
    user waits until page contains element    xpath://table//th[normalize-space()="${label}"]

user checks results table contains value
    [Arguments]    ${value}
    user waits until page contains element    xpath://table//td[normalize-space()="${value}"]

user starts creating a data block up to choosing indicators
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks
    user clicks link    Create data block
    user waits until h2 is visible    Create data block

    user waits until table tool wizard step is available    1    Select a data set
    user waits until page contains    ${SUBJECT_NAME}
    user clicks radio    ${SUBJECT_NAME}
    user clicks element    id:publicationDataStepForm-submit

    user waits until table tool wizard step is available    2    Choose locations
    user clicks element    xpath://button[@data-testid="filter-accordion-button"][contains(., "National")]
    user clicks checkbox    England
    user clicks element    id:locationFiltersForm-submit

    user waits until table tool wizard step is available    3    Choose time period
    user chooses select option    id:timePeriodForm-start    2021/22
    user chooses select option    id:timePeriodForm-end    2023/24
    user clicks element    id:timePeriodForm-submit

    user waits until table tool wizard step is available    4    Choose your filters
    user waits until page contains element    ${ENROLMENTS_LABEL}

user checks indicator checkbox is not checked
    [Arguments]    ${indicator_label}
    user checks checkbox input is not checked
    ...    xpath://*[@id="filtersForm-indicators"]//label[text()="${indicator_label}"]/../input[@type="checkbox"]
