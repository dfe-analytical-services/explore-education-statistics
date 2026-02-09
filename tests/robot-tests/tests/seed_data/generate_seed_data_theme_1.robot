*** Comments ***
#
# This test suite is responsible for setting up various Publications and Releases for the use of various Robot test
# suites, particularly those used in the general public testing, where we are more reliant on standing seed data rather
# than being free to generate new data for restricted environments.

#
# When seeding data for an environment, you will need to use the `--reseed` option of the `run_tests.py` script.
#


*** Settings ***
Library             DateTime
Library             ../libs/admin_api.py
Resource            ../libs/admin-common.robot
Resource            ../libs/admin/manage-content-common.robot
Resource            ../libs/charts.robot
Resource            seed_data_theme_1_constants.robot
Resource            seed_data_common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          SeedDataGeneration    Local    Dev    PreProd


*** Variables ***
${RELEASE_1_NAME}       ${PUPIL_ABSENCE_PUBLICATION_TITLE} ${PUPIL_ABSENCE_RELEASE_NAME}
${RELEASE_2_NAME}       ${EXCLUSIONS_PUBLICATION_TITLE} ${EXCLUSIONS_PUBLICATION_RELEASE_NAME}


*** Test Cases ***
Create test theme
    ${THEME_ID}=    user creates theme via api
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    ...    Including absence, application and offers, capacity exclusion and special educational needs (SEN) statistics
    user reloads page
    Set Suite Variable    ${THEME_ID}

Create ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    ${PUBLICATION_ID}=    user creates test publication via api
    ...    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    ...    ${THEME_ID}
    Set Suite Variable    ${PUBLICATION_ID}

    user navigates to publication page from dashboard
    ...    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Add legacy releases to ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user clicks link    Release order
    user waits until h2 is visible    Release order
    user creates legacy release    Academic year 2009/10
    ...    https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010
    user creates legacy release    Academic year 2010/11
    ...    https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011
    user creates legacy release    Academic year 2011/12
    ...    https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics
    user creates legacy release    Academic year 2012/13
    ...    https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013
    user creates legacy release    Academic year 2013/14
    ...    https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014
    user creates legacy release    Academic year 2014/15
    ...    https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015

Create ${RELEASE_1_NAME} release
    user creates test release via api
    ...    ${PUBLICATION_ID}
    ...    ${PUPIL_ABSENCE_RELEASE_TIME}
    ...    ${PUPIL_ABSENCE_RELEASE_YEAR}
    ...    OfficialStatistics

    user navigates to draft release page from dashboard
    ...    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    ...    ${PUPIL_ABSENCE_RELEASE_NAME}
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Add data files to ${RELEASE_1_NAME}
    user uploads subject and waits until complete
    ...    Absence by characteristic
    ...    absence_by_characteristic.csv
    ...    absence_by_characteristic.meta.csv
    ...    ${UNZIPPED_FILES_DIR}

    user uploads subject and waits until complete
    ...    Absence in PRUs
    ...    absence_in_prus.csv
    ...    absence_in_prus.meta.csv
    ...    ${UNZIPPED_FILES_DIR}

Add data guidance to ${RELEASE_1_NAME}
    user clicks link    Data guidance
    user waits until page finishes loading
    user enters text into element    id:dataGuidanceForm-content    Test data guidance content
    user adds data guidance for subject    Absence by characteristic    Absence by characteristic data guidance content
    user adds data guidance for subject    Absence in PRUs    Absence in PRUs data guidance content
    user clicks button    Save guidance
    user waits until page finishes loading

Create data block 1 for ${RELEASE_1_NAME}
    @{locations}=    create list    England
    @{indicators}=    create list    Authorised absence rate    Unauthorised absence rate    Overall absence rate
    @{filter_items}=    create list
    user creates data block
    ...    Absence by characteristic
    ...    2012/13
    ...    2016/17
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    Generic data block - National
    ...    'Absence by characteristic' in England between 2012/13 and 2016/17
    ...    ${EMPTY}

Create data block 2 for ${RELEASE_1_NAME}
    @{locations}=    create list    England
    @{indicators}=    create list    Overall absence rate
    @{filter_items}=    create list
    user creates data block
    ...    Absence by characteristic
    ...    2016/17
    ...    2016/17
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    Key Stat 1
    ...    Overall absence rate for 'Absence by characteristic' in England for 2016/17
    ...    ${EMPTY}

Create data block 3 for ${RELEASE_1_NAME}
    @{locations}=    create list    England
    @{indicators}=    create list    Authorised absence rate
    @{filter_items}=    create list
    user creates data block
    ...    Absence by characteristic
    ...    2016/17
    ...    2016/17
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    Key Stat 2
    ...    Authorised absence rate for 'Absence by characteristic' in England for 2016/17
    ...    ${EMPTY}

Create data block 4 for ${RELEASE_1_NAME}
    @{locations}=    create list    England
    @{indicators}=    create list    Unauthorised absence rate
    @{filter_items}=    create list
    user creates data block
    ...    Absence by characteristic
    ...    2016/17
    ...    2016/17
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    Key Stat 3
    ...    Unauthorised absence rate for 'Absence by characteristic' in England for 2016/17
    ...    ${EMPTY}

Create data block 5 for ${RELEASE_1_NAME}
    @{locations}=    create list    England
    @{indicators}=    create list    Authorised absence rate    Unauthorised absence rate    Overall absence rate
    @{filter_items}=    create list
    user creates data block
    ...    Absence by characteristic
    ...    2012/13
    ...    2016/17
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    Key Stats aggregate table
    ...    'Absence by characteristic' in England between 2012/13 and 2016/17
    ...    ${EMPTY}

Create data block 6 for ${RELEASE_1_NAME}
    # Add another data block, identical to the one above, but this time for the purpose of embedding in release content
    # whereas the one above will be chosen for the secondary statistics table.
    @{locations}=    create list    England
    @{indicators}=    create list    Authorised absence rate    Unauthorised absence rate    Overall absence rate
    @{filter_items}=    create list
    user creates data block
    ...    Absence by characteristic
    ...    2012/13
    ...    2016/17
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    Key Stats additional aggregate table
    ...    'Absence by characteristic' in England between 2012/13 and 2016/17
    ...    ${EMPTY}

Create data block 7 for ${RELEASE_1_NAME}
    @{locations}=    create list    ALL localAuthorityDistrict
    @{indicators}=    create list    Authorised absence rate    Unauthorised absence rate    Overall absence rate
    @{filter_items}=    create list
    user creates data block
    ...    Absence by characteristic
    ...    2016/17
    ...    2016/17
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    LAD map
    ...    Absence rates at Local Authority District level for 2016/17
    ...    ${EMPTY}

Add line chart to ${RELEASE_1_NAME}
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks    %{WAIT_SMALL}
    user waits until table is visible
    user clicks edit data block link    Key Stats aggregate table
    user configures basic chart    Line    500    Some alt text    A subtitle
    user selects all data sets for chart
    user clicks link    Legend
    user chooses select option    id:chartLegendConfigurationForm-position    Top
    user chooses select option    id:chartLegendConfigurationForm-items-0-symbol    Circle
    user chooses select option    id:chartLegendConfigurationForm-items-1-symbol    Circle
    user chooses select option    id:chartLegendConfigurationForm-items-2-symbol    Circle
    user saves chart configuration

Add map chart to ${RELEASE_1_NAME}
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks    %{WAIT_SMALL}
    user waits until table is visible
    user clicks edit data block link    LAD map
    user clicks link    Chart
    user waits until h3 is visible    Choose chart type
    user clicks button    Geographic
    user enters text into element    label:Alt text    Test chart alt
    user selects all data sets for chart
    user clicks link    Boundary levels
    user waits until h3 is visible    Boundary levels
    user chooses select option    name:boundaryLevel    Local Authority Districts (December 2021) UK BUC
    user saves chart configuration

Add release content to ${RELEASE_1_NAME}
    user clicks link    Content
    user waits until page finishes loading

Add release summary to ${RELEASE_1_NAME}
    user adds summary text block
    user adds content to summary text block
    ...    Read national statistical summaries, view charts and tables and download data files.

Add key statistics to ${RELEASE_1_NAME}
    user adds key statistic from data block
    ...    Key Stat 1
    ...    Up from 4.6% in 2015/16
    ...    What is overall absence?
    ...    Total number of all authorised and unauthorised absences from possible school sessions for all pupils.

    user adds key statistic from data block
    ...    Key Stat 2
    ...    Similar to previous years
    ...    What is authorized absence rate?
    ...    Number of authorised absences as a percentage of the overall school population.

    user adds key statistic from data block
    ...    Key Stat 3
    ...    Up from 1.1% in 2015/16
    ...    What is unauthorized absence rate?
    ...    Number of unauthorised absences as a percentage of the overall school population.

Add headlines to ${RELEASE_1_NAME}
    user adds headlines text block
    user adds content to headlines text block    Pupils missed on average 8.2 school days.

Add secondary statistics to ${RELEASE_1_NAME}
    user adds secondary stats data block
    ...    Key Stats aggregate table

Add content sections to ${RELEASE_1_NAME}
    user adds release content section
    ...    About these statistics
    ...    The statistics and data cover the absence of pupils of compulsory school age during the 2016/17 academic year
    ...    1

    user adds release content section
    ...    Pupil absence rates
    ...    The overall absence rate has increased across state-funded primary, secondary and special schools between 2015/16 and 2016/17.
    ...    2

    user adds data block to editable accordion section
    ...    Pupil absence rates
    ...    Generic data block - National
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user adds release content section
    ...    Persistent absence
    ...    The overall absence rate for persistent absentees across all schools increased to 18.1% - nearly 4 times higher than the rate for all pupils.
    ...    3

    user adds release content section
    ...    Reasons for absence
    ...    Illness is the main driver behind overall absence and accounted for 55.3% of all absence - down from 57.3% in 2015/16 and 60.1% in 2014/15.
    ...    4

    user adds release content section
    ...    Distribution of absence
    ...    Nearly half of all pupils (48.9%) were absent for 5 days or less across primary, secondary and special schools - down from 49.1% in 2015/16.
    ...    5

    user adds release content section
    ...    Absence by pupil characteristics
    ...    The overall absence and persistent absence patterns for pupils with different characteristics have been consistent over recent years.
    ...    6

    user adds release content section
    ...    Absence for 4-year-olds
    ...    The overall absence rate decreased to 5.1% - down from 5.2% for the previous two years.
    ...    7

    user adds release content section
    ...    Pupil referral unit absence
    ...    The overall absence rate increased to 33.9% - up from 32.6% in 2015/16.
    ...    8

    user adds release content section
    ...    Regional and local authority (LA) breakdown
    ...    Overall absence and persistent absence rates vary across primary, secondary and special schools by region and local authority (LA).
    ...    9

    user adds data block to editable accordion section
    ...    Regional and local authority (LA) breakdown
    ...    LAD map
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Add release notes to ${RELEASE_1_NAME}
    user adds a release note
    ...    First published on GOV.UK.
    ...    22
    ...    03
    ...    2018

    user adds a release note
    ...    Underlying data file updated to include absence data by pupil residency and school location, and updated metadata document.
    ...    19
    ...    04
    ...    2018

Approve ${RELEASE_1_NAME}
    user approves release for immediate publication    original    03    2019

Amend ${RELEASE_1_NAME}
    user creates amendment for release
    ...    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    ...    ${PUPIL_ABSENCE_RELEASE_NAME}
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

    # Add a release note to the amendment.
    user clicks link    Content
    user waits until page finishes loading
    user adds a release note
    ...    Updated boundary file on LAD map of absence rates.
    ...    09
    ...    03
    ...    2022

Approve ${RELEASE_1_NAME} amendment
    user approves release for immediate publication    amendment    03    2019

Backdate ${RELEASE_1_NAME} published date
    ${release_id}=    get release id from url
    ${published_override}=    Convert Date    2020-03-26 09:30:00    datetime
    user updates release published date via api    ${release_id}    ${published_override}

Create methodology ${PUPIL_ABSENCE_METHODOLOGY_TITLE}
    user creates methodology for publication
    ...    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Update summary for ${PUPIL_ABSENCE_METHODOLOGY_TITLE}
    user edits methodology summary for publication
    ...    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    ...    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    ...    ${PUPIL_ABSENCE_METHODOLOGY_TITLE}
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

    user clicks link    Manage content

Add content sections to ${PUPIL_ABSENCE_METHODOLOGY_TITLE}
    user creates new content section
    ...    1
    ...    1. Overview of absence statistics
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section
    ...    1. Overview of absence statistics
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block
    ...    1. Overview of absence statistics
    ...    1
    ...    1.1 Pupil attendance requirements for schools
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    ...    style=h3
    user adds content to accordion section text block
    ...    1. Overview of absence statistics
    ...    1
    ...    All maintained schools are required to provide 2 possible sessions per day, morning and afternoon, to all pupils.
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    ...    append=True
    user adds content to accordion section text block
    ...    1. Overview of absence statistics
    ...    1
    ...    1.2 Uses and users
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    ...    append=True
    ...    style=h3
    user adds content to accordion section text block
    ...    1. Overview of absence statistics
    ...    1
    ...    The data used to publish absence statistics is collected via the school census which is used by a variety of companies and organisations.
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    ...    append=True

    user adds methodology content section
    ...    2. National Statistics badging
    ...    Donec ullamcorper, justo vitae suscipit finibus, odio mi pretium sapien, ut finibus purus ex eget lectus. Quisque mattis augue id lacus rutrum rhoncus.
    ...    2

    user adds methodology content section
    ...    3. Methodology
    ...    Praesent faucibus facilisis arcu, non aliquet mauris finibus non. Sed pretium accumsan augue, nec pretium ipsum ullamcorper a.
    ...    3

    user adds methodology content section
    ...    4. Data collection
    ...    Pellentesque lobortis gravida dui vitae aliquam. In id nisi vel mi fringilla iaculis. Pellentesque eu aliquam diam. In blandit pretium justo, a accumsan elit efficitur eget.
    ...    4

    user adds methodology content section
    ...    5. Data processing
    ...    Duis sed dui eu est vestibulum sodales eget ullamcorper felis. Fusce pulvinar, tellus a porttitor fringilla, sem nisl auctor nibh, at porttitor nisl metus vel purus.
    ...    5

    user adds methodology content section
    ...    6. Data quality
    ...    Pellentesque condimentum bibendum ligula, sit amet feugiat nisl aliquet id. Donec eget rutrum nunc. Maecenas malesuada consectetur magna quis venenatis.
    ...    6

    user adds methodology content section
    ...    7. Contacts
    ...    If you have a specific enquiry about absence and exclusion statistics and data, contact:
    ...    7
    user opens accordion section    7. Contacts
    user adds content to accordion section text block
    ...    7. Contacts
    ...    1
    ...    School absence and exclusions team
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    ...    append=True
    ...    style=h4

Add annexes to ${PUPIL_ABSENCE_METHODOLOGY_TITLE}
    user adds methodology annex section
    ...    Annex A - Calculations
    ...    The following calculations are used to produce pupil absence National Statistics:
    ...    1

    user adds methodology annex section
    ...    Annex B - School attendance codes
    ...    Ut euismod, elit sed pretium ornare, metus elit lobortis diam, id viverra urna urna eu urna.
    ...    2

    user adds methodology annex section
    ...    Annex C - Links to pupil absence national statistics and data
    ...    Nulla sapien ex, consectetur eget ullamcorper sit amet, sodales in nisi. Aliquam eget sapien dignissim, cursus lectus et, faucibus tellus.
    ...    3

    user adds methodology annex section
    ...    Annex D - Standard breakdowns
    ...    Donec tortor lorem, vulputate eu convallis quis, euismod ac justo. Fusce vel cursus arcu. Duis mi metus, lacinia vitae hendrerit eget, vulputate ut ante.
    ...    4

    user adds methodology annex section
    ...    Annex E - Timeline
    ...    Aliquam eu augue ac ligula placerat consequat non in velit. Vivamus elementum mollis est, euismod malesuada ipsum consectetur at.
    ...    5

    user adds methodology annex section
    ...    Annex F - Absence rates over time
    ...    Vivamus ultrices rutrum erat non volutpat. Nulla euismod mattis nisl, eget aliquam ligula interdum at. Maecenas laoreet sapien eget est bibendum tempor.
    ...    6
    ...    dfe-logo.jpg

Approve ${PUPIL_ABSENCE_METHODOLOGY_TITLE}
    user approves methodology for publication
    ...    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    ...    ${PUPIL_ABSENCE_METHODOLOGY_TITLE}
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Backdate ${PUPIL_ABSENCE_METHODOLOGY_TITLE} published date
    ${methodology_id}=    get methodology id from url
    ${published_override}=    Convert Date    2018-03-22 00:00:00    datetime
    user updates methodology published date via api
    ...    ${methodology_id}
    ...    ${published_override}

Give Analyst1 Contributor access to ${RELEASE_1_NAME}
    user gives release access to analyst
    ...    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    ...    ${PUPIL_ABSENCE_RELEASE_NAME}
    ...    Contributor
    ...    EES-test.ANALYST1@education.gov.uk

Create ${EXCLUSIONS_PUBLICATION_TITLE}
    ${PUBLICATION_ID}=    user creates test publication via api
    ...    ${EXCLUSIONS_PUBLICATION_TITLE}
    ...    ${THEME_ID}

    Set Suite Variable    ${PUBLICATION_ID}

    user navigates to publication page from dashboard
    ...    ${EXCLUSIONS_PUBLICATION_TITLE}
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Add legacy releases to ${EXCLUSIONS_PUBLICATION_TITLE}
    user clicks link    Release order
    user waits until h2 is visible    Release order
    user creates legacy release    Academic year 2008/09
    ...    https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-academic-year-2008-to-2009
    user creates legacy release    Academic year 2009/10
    ...    https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2009-to-2010
    user creates legacy release    Academic year 2010/11
    ...    https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-academic-year-2010-to-2011
    user creates legacy release    Academic year 2011/12
    ...    https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-from-schools-in-england-2011-to-2012-academic-year
    user creates legacy release    Academic year 2012/13
    ...    https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2012-to-2013
    user creates legacy release    Academic year 2013/14
    ...    https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2013-to-2014
    user creates legacy release    Academic year 2014/15
    ...    https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2014-to-2015
    user creates legacy release    Academic year 2015/16
    ...    https://www.gov.uk/government/statistics/permanent-and-fixed-period-exclusions-in-england-2015-to-2016

Create ${RELEASE_2_NAME} release
    user creates test release via api
    ...    ${PUBLICATION_ID}
    ...    ${EXCLUSIONS_PUBLICATION_RELEASE_TIME}
    ...    ${EXCLUSIONS_PUBLICATION_RELEASE_YEAR}
    ...    OfficialStatistics

    user navigates to draft release page from dashboard
    ...    ${EXCLUSIONS_PUBLICATION_TITLE}
    ...    ${EXCLUSIONS_PUBLICATION_RELEASE_NAME}
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Add data files to ${RELEASE_2_NAME}
    user uploads subject and waits until complete
    ...    Exclusions by geographic level
    ...    exclusions_by_geographic_level.csv
    ...    exclusions_by_geographic_level.meta.csv
    ...    ${UNZIPPED_FILES_DIR}

Add data guidance to ${RELEASE_2_NAME}
    user clicks link    Data guidance
    user waits until page finishes loading
    user enters text into element    id:dataGuidanceForm-content    Test data guidance content
    user adds data guidance for subject    Exclusions by geographic level
    ...    Exclusions by geographic level data guidance content
    user clicks button    Save guidance
    user waits until page finishes loading

Create data block 1 for ${RELEASE_2_NAME}
    @{locations}=    create list    England
    @{indicators}=    create list    Number of pupils    Number of permanent exclusions    Permanent exclusion rate
    @{filter_items}=    create list
    user creates data block
    ...    Exclusions by geographic level
    ...    2012/13
    ...    2016/17
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    Generic data block 1
    ...    Chart showing permanent exclusions in England

Create data block 2 for ${RELEASE_2_NAME}
    @{locations}=    create list    England
    @{indicators}=    create list    Number of pupils    Number of fixed period exclusions
    ...    Fixed period exclusion rate
    @{filter_items}=    create list
    user creates data block
    ...    Exclusions by geographic level
    ...    2012/13
    ...    2016/17
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    Generic data block 2
    ...    Chart showing fixed-period exclusions in England

Create data block 3 for ${RELEASE_2_NAME}
    @{locations}=    create list    England
    @{indicators}=    create list    Permanent exclusion rate
    @{filter_items}=    create list
    user creates data block
    ...    Exclusions by geographic level
    ...    2016/17
    ...    2016/17
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    Key Stat 1

Create data block 4 for ${RELEASE_2_NAME}
    @{locations}=    create list    England
    @{indicators}=    create list    Fixed period exclusion rate
    @{filter_items}=    create list
    user creates data block
    ...    Exclusions by geographic level
    ...    2016/17
    ...    2016/17
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    Key Stat 2

Create data block 5 for ${RELEASE_2_NAME}
    @{locations}=    create list    England
    @{indicators}=    create list    Number of permanent exclusions
    @{filter_items}=    create list
    user creates data block
    ...    Exclusions by geographic level
    ...    2016/17
    ...    2016/17
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    Key Stat 3

Create data block 6 for ${RELEASE_2_NAME}
    @{locations}=    create list    England
    @{indicators}=    create list    Permanent exclusion rate    Fixed period exclusion rate
    ...    Number of permanent exclusions
    @{filter_items}=    create list
    user creates data block
    ...    Exclusions by geographic level
    ...    2012/13
    ...    2016/17
    ...    ${locations}
    ...    ${indicators}
    ...    ${filter_items}
    ...    Key Stats aggregate table
    ...    'Exclusions by geographic level' in England between 2012/13 and 2016/17
    ...    ${EMPTY}

Add line chart 1 to ${RELEASE_2_NAME}
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks    %{WAIT_SMALL}
    user waits until table is visible
    user clicks edit data block link    Generic data block 1
    user configures basic chart    Line    500    Some alt text    A subtitle
    user selects all data sets for chart
    user clicks link    Legend
    user chooses select option    id:chartLegendConfigurationForm-position    Top
    user chooses select option    id:chartLegendConfigurationForm-items-0-symbol    Circle
    user chooses select option    id:chartLegendConfigurationForm-items-1-symbol    Circle
    user chooses select option    id:chartLegendConfigurationForm-items-2-symbol    Circle
    user saves chart configuration

Add line chart 2 to ${RELEASE_2_NAME}
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks    %{WAIT_SMALL}
    user waits until table is visible
    user clicks edit data block link    Generic data block 2
    user configures basic chart    Line    500    Some alt text    A subtitle
    user selects all data sets for chart
    user clicks link    Legend
    user chooses select option    id:chartLegendConfigurationForm-position    Top
    user chooses select option    id:chartLegendConfigurationForm-items-0-symbol    Circle
    user chooses select option    id:chartLegendConfigurationForm-items-1-symbol    Circle
    user chooses select option    id:chartLegendConfigurationForm-items-2-symbol    Circle
    user saves chart configuration

Add line chart 3 to ${RELEASE_2_NAME}
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks    %{WAIT_SMALL}
    user waits until table is visible
    user clicks edit data block link    Key Stats aggregate table
    user configures basic chart    Line    500    Some alt text    A subtitle
    user selects all data sets for chart
    user clicks link    Legend
    user chooses select option    id:chartLegendConfigurationForm-position    Top
    user chooses select option    id:chartLegendConfigurationForm-items-0-symbol    Circle
    user chooses select option    id:chartLegendConfigurationForm-items-1-symbol    Circle
    user chooses select option    id:chartLegendConfigurationForm-items-2-symbol    Circle
    user saves chart configuration

Add release content to ${RELEASE_2_NAME}
    user clicks link    Content
    user waits until page finishes loading

Add release summary to ${RELEASE_2_NAME}
    user adds summary text block
    user adds content to summary text block
    ...    Read national statistical summaries, view charts and tables and download data files.

Add key statistics to ${RELEASE_2_NAME}
    user adds key statistic from data block
    ...    Key Stat 1
    ...    Up from 0.08% in 2015/16
    ...    What is permanent exclusion rate?
    ...    Number of permanent exclusions as a percentage of the overall school population.

    user adds key statistic from data block
    ...    Key Stat 2
    ...    Up from 4.29% in 2015/16
    ...    What is fixed period exclusion rate?
    ...    Number of fixed-period exclusions as a percentage of the overall school population.

    user adds key statistic from data block
    ...    Key Stat 3
    ...    Up from 6,685 in 2015/16
    ...    What is number of permanent exclusions?
    ...    Total number of permanent exclusions within a school year.

Add headlines to ${RELEASE_2_NAME}
    user adds headlines text block
    user adds content to headlines text block
    ...    The rate of permanent exclusions has increased since last year from 0.08 per cent of pupil enrolments in 2015/16 to 0.10 per cent in 2016/17.

Add secondary statistics to ${RELEASE_2_NAME}
    user adds secondary stats data block
    ...    Key Stats aggregate table

Add content sections to ${RELEASE_2_NAME}
    user adds release content section
    ...    About this release
    ...    The statistics and data cover permanent and fixed period exclusions and school-level exclusions during the 2016/17 academic year in the following state-funded school types as reported in the school census.
    ...    1

    user adds release content section
    ...    Permanent exclusions
    ...    The number of permanent exclusions has increased across all state-funded primary, secondary and special schools to 7,720 - up from 6,685 in 2015/16.
    ...    2

    user adds data block to editable accordion section
    ...    Permanent exclusions
    ...    Generic data block 1
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user adds release content section
    ...    Fixed-period exclusions
    ...    The number of fixed-period exclusions has increased across all state-funded primary, secondary and special schools to 381,865 - up from 339,360 in 2015/16.
    ...    3

    user adds data block to editable accordion section
    ...    Fixed-period exclusions
    ...    Generic data block 2
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user adds release content section
    ...    Number and length of fixed-period exclusions
    ...    The number of pupils with one or more fixed-period exclusion has increased across state-funded primary, secondary and special schools to 183,475 (2.29% of pupils) up from 167,125 (2.11% of pupils) in 2015/16.
    ...    4

    user adds release content section
    ...    Reasons for exclusions
    ...    All reasons (except bullying and theft) saw an increase in permanent exclusions since 2015/16.
    ...    5

    user adds release content section
    ...    Exclusions by pupil characteristics
    ...    There was a similar pattern to previous years where the following groups (where higher exclusion rates are expected) showed an increase in exclusions since 2015/16.
    ...    6

    user adds release content section
    ...    Independent exclusion reviews
    ...    There were 560 reviews lodged with independent review panels in maintained primary, secondary and special schools and academies of which 525 (93.4%) were determined and 45 (8.0%) resulted in an offer of reinstatement.
    ...    7

    user adds release content section
    ...    Pupil referral units exclusions
    ...    The permanent exclusion rate in pupil referral units decreased to 0.13 - down from 0.14% in 2015/16.
    ...    8

    user adds release content section
    ...    Regional and local authority (LA) breakdown
    ...    There's considerable variation in the permanent exclusion and fixed-period exclusion rate at the LA level.
    ...    9

Add release notes to ${RELEASE_2_NAME}
    user adds a release note
    ...    First published on GOV.UK.
    ...    19
    ...    07
    ...    2018

    user adds a release note
    ...    Updated exclusion rates for Gypsy/Roma pupils, to include extended ethnicity categories within the headcount (Gypsy, Roma and other Gypsy/Roma).
    ...    25
    ...    08
    ...    2018

Approve ${RELEASE_2_NAME}
    user approves release for immediate publication    original    07    2019

Backdate ${RELEASE_2_NAME} published date
    ${release_id}=    get release id from url
    ${published_override}=    Convert Date    2020-03-26 09:30:00    datetime
    user updates release published date via api    ${release_id}    ${published_override}

Create methodology ${EXCLUSIONS_METHODOLOGY_TITLE}
    user creates methodology for publication
    ...    ${EXCLUSIONS_PUBLICATION_TITLE}
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Update summary for ${EXCLUSIONS_METHODOLOGY_TITLE}
    user edits methodology summary for publication
    ...    ${EXCLUSIONS_PUBLICATION_TITLE}
    ...    ${EXCLUSIONS_PUBLICATION_TITLE}
    ...    ${EXCLUSIONS_METHODOLOGY_TITLE}
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

    user clicks link    Manage content

Add content sections to ${EXCLUSIONS_METHODOLOGY_TITLE}
    user adds methodology content section
    ...    1. Overview of exclusion statistics
    ...    The headteacher of a school can exclude a pupil on disciplinary grounds only.
    ...    1

    user adds methodology content section
    ...    2. National Statistics badging
    ...    Donec ullamcorper, justo vitae suscipit finibus, odio mi pretium sapien, ut finibus purus ex eget lectus. Quisque mattis augue id lacus rutrum rhoncus.
    ...    2

    user adds methodology content section
    ...    3. Methodology
    ...    Praesent faucibus facilisis arcu, non aliquet mauris finibus non. Sed pretium accumsan augue, nec pretium ipsum ullamcorper a.
    ...    3

    user adds methodology content section
    ...    4. Data collection
    ...    Pellentesque lobortis gravida dui vitae aliquam. In id nisi vel mi fringilla iaculis. Pellentesque eu aliquam diam. In blandit pretium justo, a accumsan elit efficitur eget.
    ...    4

    user adds methodology content section
    ...    5. Data processing
    ...    Duis sed dui eu est vestibulum sodales eget ullamcorper felis. Fusce pulvinar, tellus a porttitor fringilla, sem nisl auctor nibh, at porttitor nisl metus vel purus.
    ...    5

    user adds methodology content section
    ...    6. Data quality
    ...    Pellentesque condimentum bibendum ligula, sit amet feugiat nisl aliquet id. Donec eget rutrum nunc. Maecenas malesuada consectetur magna quis venenatis.
    ...    6

    user adds methodology content section
    ...    7. Contacts
    ...    If you have a specific enquiry about absence and exclusion statistics and data, contact:
    ...    7

Add annexes to ${EXCLUSIONS_METHODOLOGY_TITLE}
    user adds methodology annex section
    ...    Annex A - Calculations
    ...    The following calculations are used to produce pupil absence National Statistics:
    ...    1

    user adds methodology annex section
    ...    Annex B - Exclusion by reason codes
    ...    Ut euismod, elit sed pretium ornare, metus elit lobortis diam, id viverra urna urna eu urna.
    ...    2

    user adds methodology annex section
    ...    Annex C - Links to pupil exclusions statistics and data
    ...    Nulla sapien ex, consectetur eget ullamcorper sit amet, sodales in nisi. Aliquam eget sapien dignissim, cursus lectus et, faucibus tellus.
    ...    3

    user adds methodology annex section
    ...    Annex D - Standard breakdowns
    ...    Donec tortor lorem, vulputate eu convallis quis, euismod ac justo. Fusce vel cursus arcu. Duis mi metus, lacinia vitae hendrerit eget, vulputate ut ante.
    ...    4

Approve ${EXCLUSIONS_METHODOLOGY_TITLE}
    user approves methodology for publication
    ...    ${EXCLUSIONS_PUBLICATION_TITLE}
    ...    ${EXCLUSIONS_METHODOLOGY_TITLE}
    ...    ${PUPILS_AND_SCHOOLS_THEME_TITLE}

Backdate ${EXCLUSIONS_METHODOLOGY_TITLE} published date
    ${methodology_id}=    get methodology id from url
    ${published_override}=    Convert Date    2018-08-25 00:00:00    datetime
    user updates methodology published date via api
    ...    ${methodology_id}
    ...    ${published_override}

Give Analyst1 Contributor access to ${RELEASE_2_NAME}
    user gives release access to analyst
    ...    ${EXCLUSIONS_PUBLICATION_TITLE}
    ...    ${EXCLUSIONS_PUBLICATION_RELEASE_NAME}
    ...    Contributor
    ...    EES-test.ANALYST1@education.gov.uk


*** Keywords ***
user adds release content section
    [Arguments]
    ...    ${section_title}
    ...    ${section_text}
    ...    ${section_num}
    user creates new content section
    ...    ${section_num}
    ...    ${section_title}
    user adds text block to editable accordion section
    ...    ${section_title}
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds content to autosaving accordion section text block
    ...    ${section_title}
    ...    1
    ...    ${section_text}
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

user adds methodology content section
    [Arguments]
    ...    ${section_title}
    ...    ${section_text}
    ...    ${section_num}

    user creates new content section
    ...    ${section_num}
    ...    ${section_title}
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section
    ...    ${section_title}
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block
    ...    ${section_title}
    ...    1
    ...    ${section_text}
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

user adds methodology annex section
    [Arguments]
    ...    ${section_title}
    ...    ${section_text}
    ...    ${section_num}
    ...    ${image_filename}=${EMPTY}

    user creates new content section
    ...    ${section_num}
    ...    ${section_title}
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

    user adds text block to editable accordion section
    ...    ${section_title}
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

    user adds content to accordion section text block
    ...    ${section_title}
    ...    1
    ...    ${section_text}
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

    IF    "${image_filename}" != "${EMPTY}"
        user adds image to accordion section text block
        ...    ${section_title}
        ...    1
        ...    ${image_filename}
        ...    Alt text for the uploaded annex image
        ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    END
