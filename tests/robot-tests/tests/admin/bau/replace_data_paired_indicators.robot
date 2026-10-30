*** Settings ***
Resource            ../../libs/admin-common.robot
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
${INDICATORS_SEARCH}            id:filtersForm-indicators-search
${ENROLMENTS_LABEL}             ${INDICATORS}//label[text()="Number of pupil enrolments"]
${OVERALL_SESSIONS_LABEL}       ${INDICATORS}//label[text()="Overall absence sessions"]
${AUTHORISED_SESSIONS_LABEL}    ${INDICATORS}//label[text()="Authorised absence sessions"]
${AUTHORISED_RATE_LABEL}        ${INDICATORS}//label[text()="Authorised absence rate"]
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

Check a child indicator is selected with its parent in the table tool
    user starts creating a data block up to choosing indicators

    user waits until page contains    Selected automatically with Overall absence sessions
    user waits until page contains    Also selects Overall absence rate
    user checks element is disabled    ${OVERALL_RATE_CHECKBOX}
    user checks indicator checkbox is not checked    Overall absence rate

    user clicks indicator checkbox    Overall absence sessions
    user checks indicator checkbox is checked    Overall absence sessions
    user checks indicator checkbox is checked    Overall absence rate

    user clicks indicator checkbox    Overall absence sessions
    user checks indicator checkbox is not checked    Overall absence sessions
    user checks indicator checkbox is not checked    Overall absence rate

Check searching for a child indicator also shows its parent in the table tool
    user enters text into element    ${INDICATORS_SEARCH}    Authorised absence rate

    user waits until page contains element    ${AUTHORISED_SESSIONS_LABEL}
    user waits until page contains element    ${AUTHORISED_RATE_LABEL}
    user waits until page does not contain element    ${ENROLMENTS_LABEL}
    user waits until page does not contain element    ${OVERALL_SESSIONS_LABEL}

    user clears element text    ${INDICATORS_SEARCH}
    user waits until page contains element    ${ENROLMENTS_LABEL}

Check selecting all parent indicators also selects their children in the table tool
    user clicks select all for category    Absence sessions

    user checks indicator checkbox is checked    Overall absence sessions
    user checks indicator checkbox is checked    Authorised absence sessions
    user checks indicator checkbox is checked    Unauthorised absence sessions
    user checks indicator checkbox is checked    Overall absence rate
    user checks indicator checkbox is checked    Authorised absence rate
    user checks indicator checkbox is checked    Unauthorised absence rate

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

    # The renamed child indicator isn't mapped automatically, so the pair is dropped
    user checks table cell contains    2    1    Authorised absence sessions    ${REPLACEMENT_PAIRS_TABLE}
    user checks table cell contains    2    2    Authorised absence rate    ${REPLACEMENT_PAIRS_TABLE}
    user checks table cell contains    2    3    Dropped    ${REPLACEMENT_PAIRS_TABLE}

    # The removed child indicator has nothing to map to, so the pair is dropped
    user checks table cell contains    3    1    Unauthorised absence sessions    ${REPLACEMENT_PAIRS_TABLE}
    user checks table cell contains    3    2    Unauthorised absence rate    ${REPLACEMENT_PAIRS_TABLE}
    user checks table cell contains    3    3    Dropped    ${REPLACEMENT_PAIRS_TABLE}

Check dropped pairs don't block the data replacement
    user waits until page contains    Data blocks: OK
    user waits until page contains    Footnotes: OK
    user waits until page contains button    Confirm data replacement

Follow the link to manage paired indicators
    user clicks link    Manage paired indicators
    user waits until h2 is visible    Manage filters and indicators
    user waits until h3 is visible    Manage indicators for ${SUBJECT_NAME}
    user checks table body has x rows    3    id:reordering
    user clicks button    Back to data files    id:reordering

Confirm data replacement
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

    # The overall pair was maintained, so its child still can't be selected on its own
    user checks element is disabled    ${OVERALL_RATE_CHECKBOX}


*** Keywords ***
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
