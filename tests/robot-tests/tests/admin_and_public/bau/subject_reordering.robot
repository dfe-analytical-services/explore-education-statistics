*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/public-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}     UI tests - subject reordering %{RUN_IDENTIFIER}
${RELEASE_NAME}         Calendar year 2000


*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    CY    2000

Navigate to release
    user navigates to draft release page from dashboard
    ...    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Upload subjects to release
    user uploads subject and waits until complete
    ...    Four
    ...    ordering-test-4.csv
    ...    ordering-test-4.meta.csv

    user uploads subject and waits until complete
    ...    Three
    ...    ordering-test-3.csv
    ...    ordering-test-3.meta.csv

    user uploads subject and waits until complete
    ...    One
    ...    ordering-test-1.csv
    ...    ordering-test-1.meta.csv

    user uploads subject and waits until complete
    ...    Two
    ...    ordering-test-2.csv
    ...    ordering-test-2.meta.csv

Validate order of subjects after upload
    user waits until page contains data uploads table
    user checks table body has x rows    count=4    parent=testid:Data files table

    user checks table cell contains    row=1    column=1    expected=Four    parent=testid:Data files table
    user checks table cell contains    row=2    column=1    expected=Three    parent=testid:Data files table
    user checks table cell contains    row=3    column=1    expected=One    parent=testid:Data files table
    user checks table cell contains    row=4    column=1    expected=Two    parent=testid:Data files table

Validate order of subjects after refreshing Data and files page
    user reloads page
    user waits until page contains data uploads table
    user checks table body has x rows    count=4    parent=testid:Data files table

    user checks table cell contains    row=1    column=1    expected=Four    parent=testid:Data files table
    user checks table cell contains    row=2    column=1    expected=Three    parent=testid:Data files table
    user checks table cell contains    row=3    column=1    expected=One    parent=testid:Data files table
    user checks table cell contains    row=4    column=1    expected=Two    parent=testid:Data files table

Order subjects
    user clicks button    Reorder data files
    user waits until page contains button    Confirm order

    click element    xpath://div[text()="Four"]    CTRL
    user presses keys    ${SPACE}
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN
    user presses keys    ${SPACE}

    click element    xpath://div[text()="Three"]    CTRL
    user presses keys    ${SPACE}
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN
    user presses keys    ${SPACE}

    user clicks button    Confirm order
    user waits until page contains button    Reorder

    user checks table cell contains    row=1    column=1    expected=One    parent=testid:Data files table
    user checks table cell contains    row=2    column=1    expected=Two    parent=testid:Data files table
    user checks table cell contains    row=3    column=1    expected=Four    parent=testid:Data files table
    user checks table cell contains    row=4    column=1    expected=Three    parent=testid:Data files table

Validate new order is preserved after refresh
    user reloads page
    user waits until page contains data uploads table

    user checks table cell contains    row=1    column=1    expected=One    parent=testid:Data files table
    user checks table cell contains    row=2    column=1    expected=Two    parent=testid:Data files table
    user checks table cell contains    row=3    column=1    expected=Four    parent=testid:Data files table
    user checks table cell contains    row=4    column=1    expected=Three    parent=testid:Data files table

Start replacing last subject in order
    user clicks link in table cell    row=4    column=4    link_text=Replace data    parent=testid:Data files table
    user chooses file    id:dataFileUploadForm-dataFile    ${FILES_DIR}ordering-test-3-replacement.csv
    user chooses file    id:dataFileUploadForm-metadataFile
    ...    ${FILES_DIR}ordering-test-3-replacement.meta.csv
    user clicks button    Upload data files

    user waits until page contains element    testid:Replacement Subject title
    user checks headed table body row cell contains    Subject title    1    Three
    user checks headed table body row cell contains    Data file    1    ordering-test-3.csv
    user checks headed table body row cell contains    Subject title    2    Three
    user checks headed table body row cell contains    Data file    2    ordering-test-3-replacement.csv
    user checks headed table body row cell contains    Status    2    Complete    wait=%{WAIT_DATA_FILE_IMPORT}

Reorder subject that is being replaced
    user clicks link    Data and files
    user waits until page contains data uploads table

    user clicks button    Reorder data files
    user waits until page contains button    Confirm order

    click element    xpath://div[text()="Three"]    CTRL
    user presses keys    ${SPACE}
    user presses keys    ARROW_UP
    user presses keys    ${SPACE}

    user clicks button    Confirm order
    user waits until page contains button    Reorder data files

    user checks table cell contains    row=1    column=1    expected=One    parent=testid:Data files table
    user checks table cell contains    row=2    column=1    expected=Two    parent=testid:Data files table
    user checks table cell contains    row=3    column=1    expected=Three    parent=testid:Data files table
    user checks table cell contains    row=4    column=1    expected=Four    parent=testid:Data files table

Complete data replacement
    user clicks link in table cell    row=3    column=4    link_text=Replace data    parent=testid:Data files table

    user waits until page contains    Data blocks: OK
    user waits until page contains    Footnotes: OK
    user clicks button    Confirm data replacement
    user waits until h2 is visible    Data replacement complete

Validate subject order is correct after replacement
    user clicks link    Data and files
    user waits until page contains data uploads table

    user checks table cell contains    row=1    column=1    expected=One    parent=testid:Data files table
    user checks table cell contains    row=2    column=1    expected=Two    parent=testid:Data files table
    user checks table cell contains    row=3    column=1    expected=Three    parent=testid:Data files table
    user checks table cell contains    row=4    column=1    expected=Four    parent=testid:Data files table

Add data guidance for all subjects
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    One

    user waits until page contains element    id:dataGuidance-dataFiles
    user checks accordion is in position    One    1    id:dataGuidance-dataFiles
    user checks accordion is in position    Two    2    id:dataGuidance-dataFiles
    user checks accordion is in position    Three    3    id:dataGuidance-dataFiles
    user checks accordion is in position    Four    4    id:dataGuidance-dataFiles

    user enters text into data guidance data file content editor    One
    ...    One guidance content
    user enters text into data guidance data file content editor    Two
    ...    Two guidance content
    user enters text into data guidance data file content editor    Three
    ...    Three guidance content
    user enters text into data guidance data file content editor    Four
    ...    Four guidance content

    user clicks button    Save guidance

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Publish release
    user approves original release for immediate publication

Check subjects can no longer be re-ordered after release has been published
    user clicks link    Data and files
    user waits until page contains data uploads table

    user checks table cell contains    row=1    column=1    expected=One    parent=testid:Data files table
    user checks table cell contains    row=2    column=1    expected=Two    parent=testid:Data files table
    user checks table cell contains    row=3    column=1    expected=Three    parent=testid:Data files table
    user checks table cell contains    row=4    column=1    expected=Four    parent=testid:Data files table

    user checks element is not visible    testid:reorder-files    %{WAIT_SMALL}

Check subject order in data tables
    user navigates to data tables page on public frontend

    user clicks radio    %{TEST_THEME_NAME}
    user clicks radio    ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit

    user waits until table tool wizard step is available    2    Select a data set
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME}

    user checks radio in position has label    1    One
    user checks radio in position has label    2    Two
    user checks radio in position has label    3    Three
    user checks radio in position has label    4    Four

Check subject order in data catalogue
    user navigates to data catalogue page on public frontend

    user wait for option to be available and select it    css:select[id="filters-form-theme"]    %{TEST_THEME_NAME}
    user wait for option to be available and select it    css:select[id="filters-form-publication"]
    ...    ${PUBLICATION_NAME}
    sleep    1    # wait a moment to wait for release filter options to get updated
    user wait for option to be available and select it    css:select[id="filters-form-release"]    ${RELEASE_NAME}

    user waits until page contains    Download all 4 data sets (ZIP)

    user checks items matching locator contain exact items in order
    ...    One
    ...    Two
    ...    Three
    ...    Four
    ...    locator=xpath://*[@data-testid="data-set-file-list"]/li/h4

Check subject order in data guidance
    user checks publication is on find statistics page    ${PUBLICATION_NAME}
    user clicks link    ${PUBLICATION_NAME}

    user waits until h1 is visible    ${PUBLICATION_NAME}    %{WAIT_MEDIUM}
    user clicks link    Data guidance

    user waits until page contains element    id:dataFiles
    user checks accordion is in position    One    1    id:dataFiles
    user checks accordion is in position    Two    2    id:dataFiles
    user checks accordion is in position    Three    3    id:dataFiles
    user checks accordion is in position    Four    4    id:dataFiles
