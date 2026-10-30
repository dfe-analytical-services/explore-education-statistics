*** Settings ***
Resource            ../../libs/admin-common.robot
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
    user waits until page contains data uploads table
    user waits until page contains element    testid:Data file replacements table
    user clicks link in table cell    1    4    View details    testid:Data file replacements table
    user waits until h3 is visible    Paired indicators    %{WAIT_MEDIUM}
    user clicks button    Confirm data replacement
    user waits until h2 is visible    Data replacement complete

Check only the maintained pair remains after data replacement
    user clicks link    Data and files
    user clicks link    Manage filters and indicators
    user waits until h2 is visible    Manage filters and indicators
    user waits until table is visible    id:reordering
    user clicks button in table cell    1    2    Manage indicators    id:reordering
    user waits until h3 is visible    Manage indicators for ${SUBJECT_NAME}

    user checks table body has x rows    1    id:reordering
    user checks table cell contains    1    1    Overall absence sessions    id:reordering
    user checks table cell contains    1    2    Overall absence rate    id:reordering
