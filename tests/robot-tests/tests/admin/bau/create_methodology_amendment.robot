*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}=    UI tests - create methodology amendment publication %{RUN_IDENTIFIER}


*** Test Cases ***
Create publicly accessible Publication
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2021
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Academic year 2021/22
    user approves original release for immediate publication

Create Methodology with some content and images
    user creates methodology for publication    ${PUBLICATION_NAME}
    user edits methodology summary for publication
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - first methodology version

    user clicks link    Manage content
    user creates new content section    1    Methodology content section 1    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology content section 1    1
    ...    Adding Methodology content    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds image to accordion section text block    Methodology content section 1    1    test-infographic.png
    ...    Alt text for the uploaded content image    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

    user creates new content section    1    Methodology annex section 1    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology annex section 1
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology annex section 1    1    Adding Methodology annex
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds image to accordion section text block    Methodology annex section 1    1    dfe-logo.jpg
    ...    Alt text for the uploaded annex image    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

    user creates new content section    2    Methodology annex section 2    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology annex section 2
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology annex section 2    1
    ...    Adding Methodology annex 2 text block 1    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds image to accordion section text block    Methodology annex section 2    1    dfe-logo.jpg
    ...    Alt text for the uploaded annex image 2    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

    user adds text block to editable accordion section    Methodology annex section 2
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology annex section 2    2
    ...    Adding Methodology annex 2 text block 2    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user adds image to accordion section text block    Methodology annex section 2    2    test-infographic.png
    ...    Alt text for the uploaded annex image 3    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

Verify the editable content is as expected
    user checks accordion section contains x blocks    Methodology content section 1    1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

    user checks accordion section text block contains    Methodology content section 1    1
    ...    Adding Methodology content    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user checks accordion section text block contains image with alt text    Methodology content section 1    1
    ...    Alt text for the uploaded content image    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

    user checks accordion section contains x blocks    Methodology annex section 1    1
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

    user checks accordion section text block contains    Methodology annex section 1    1    Adding Methodology annex
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user checks accordion section text block contains image with alt text    Methodology annex section 1    1
    ...    Alt text for the uploaded annex image    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

    user checks accordion section contains x blocks    Methodology annex section 2    2
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

    user checks accordion section text block contains    Methodology annex section 2    1
    ...    Adding Methodology annex 2 text block 1    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user checks accordion section text block contains image with alt text    Methodology annex section 2    1
    ...    Alt text for the uploaded annex image 2    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

    user checks accordion section text block contains    Methodology annex section 2    2
    ...    Adding Methodology annex 2 text block 2    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user checks accordion section text block contains image with alt text    Methodology annex section 2    2
    ...    Alt text for the uploaded annex image 3    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

Approve the Methodology
    user approves methodology for publication    ${PUBLICATION_NAME}    ${PUBLICATION_NAME} - first methodology version

Verify the summary for the original Methodology is as expected
    ${expected_published_date}=    get current datetime    %-d %B %Y
    user navigates to methodologies on publication page
    ...    ${PUBLICATION_NAME}

    ${ROW}=    user gets table row    ${PUBLICATION_NAME} - first methodology version    testid:methodologies
    user clicks element    xpath://*[text()="View"]    ${ROW}

    user verifies methodology summary details
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - first methodology version
    ...    Approved
    ...    ${expected_published_date}

Verify the readonly content for the original Methodology is as expected
    user clicks link    Manage content
    user verifies original Methodology readonly content

Create a Methodology Amendment
    user creates methodology amendment for publication    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - first methodology version
    user checks page contains tag    Draft
    user checks page contains tag    Amendment

Edit the Amendment's summary to return the Methodology's title to the same as its owning Publication's title
    user edits methodology summary for publication
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME} - first methodology version
    ...    ${PUBLICATION_NAME}
    user verifies methodology summary details
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    ...    Draft
    ...    Not yet published
    user checks page contains tag    Amendment
    user clicks link    Edit summary
    user waits until h2 is visible    Edit methodology summary
    # Double check that the front end is now showing the Methodology's title as being "Use publication title" when
    # visiting the "Edit methodology summary" page again.
    user checks radio is checked    Use publication title

Remove a Content Section from the Amendment
    user clicks link    Manage content
    user opens accordion section    Methodology annex section 1    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user deletes editable accordion section    Methodology annex section 1    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

Remove an image from a Content Block
    user closes Set Page View box
    user opens accordion section    Methodology annex section 2    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user removes image from accordion section text block    Methodology annex section 2    2
    ...    Alt text for the uploaded annex image 3    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}    Save
    user checks accordion section text block does not contain image with alt text    Methodology annex section 2    2
    ...    Alt text for the uploaded content image    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

Remove a Content Block from a Content Section
    user deletes editable accordion section content block    Methodology annex section 2    1
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user checks accordion section contains x blocks    Methodology annex section 2    1
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

Add a new Content Block to an existing Content Section and include a new image
    user opens accordion section    Methodology content section 1    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds image to accordion section text block    Methodology content section 1    2    gov-uk.png
    ...    Alt text for the uploaded content image 2    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user checks accordion section contains x blocks    Methodology content section 1    2
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

Verify all is as expected on the Amendment after the Amendment content changes
    user checks accordion section contains x blocks    Methodology content section 1    2
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user checks accordion section text block contains image with alt text    Methodology content section 1    1
    ...    Alt text for the uploaded content image    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user checks accordion section text block contains image with alt text    Methodology content section 1    2
    ...    Alt text for the uploaded content image 2    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user checks accordion section contains x blocks    Methodology annex section 2    1
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

Revisit the original Methodology and check that its content remains unaffected by the changes to the Amendment
    user navigates to methodologies on publication page    ${PUBLICATION_NAME}

    ${ROW}=    user gets table row    ${PUBLICATION_NAME}    testid:methodologies
    user clicks element    xpath://*[text()="View existing version"]    ${ROW}
    user waits until h2 is visible    Methodology summary

    user clicks link    Manage content
    user verifies original Methodology readonly content

Approve the Methodology Amendment
    user approves methodology amendment for publication    ${PUBLICATION_NAME}

Revisit the Publication methodologies page and check that the new Amendment is now the live Methodology
    user navigates to methodologies on publication page    ${PUBLICATION_NAME}

    ${ROW}=    user gets table row    ${PUBLICATION_NAME}    testid:methodologies

    user checks element contains link    ${ROW}    View
    user checks element does not contain link    ${ROW}    Edit
    user checks element contains button    ${ROW}    Amend
    user checks element does not contain link    ${ROW}    View existing version

Visit the approved Amendment and check that its summary is as expected
    ${ROW}=    user gets table row    ${PUBLICATION_NAME}    testid:methodologies
    user clicks element    xpath://*[text()="View"]    ${ROW}
    user waits until h2 is visible    Methodology summary

    ${date}=    get current datetime    %-d %B %Y
    user verifies methodology summary details
    ...    ${PUBLICATION_NAME}
    ...    ${PUBLICATION_NAME}
    ...    Approved
    ...    ${date}

Check the approved Amendment's readonly content is as expected
    user clicks link    Manage content
    user verifies amended Methodology readonly content

Create and cancel an Amendment
    user creates methodology amendment for publication    ${PUBLICATION_NAME}
    user cancels methodology amendment for publication    ${PUBLICATION_NAME}

    ${ROW}=    user gets table row    ${PUBLICATION_NAME}    testid:methodologies

    user checks element contains link    ${ROW}    View
    user checks element does not contain link    ${ROW}    Edit
    user checks element contains button    ${ROW}    Amend

Revisit the live Amendment after the cancellation to double check it remains unaffected
    ${ROW}=    user gets table row    ${PUBLICATION_NAME}    testid:methodologies
    user clicks element    xpath://*[text()="View"]    ${ROW}
    user waits until h2 is visible    Methodology summary

    user clicks link    Manage content
    user verifies amended Methodology readonly content


*** Keywords ***
user verifies original Methodology readonly content
    ${section}=    user opens accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_READONLY_ACCORDION}
    user verifies accordion is open    Methodology content section 1
    user waits until element contains    ${section}    Adding Methodology content    %{WAIT_MEDIUM}
    user waits until parent contains element    ${section}
    ...    xpath://img[@alt="Alt text for the uploaded content image"]    %{WAIT_MEDIUM}

    ${section}=    user opens accordion section    Methodology annex section 1
    ...    ${METHODOLOGY_ANNEXES_READONLY_ACCORDION}
    user verifies accordion is open    Methodology annex section 1
    user waits until element contains    ${section}    Adding Methodology annex
    user waits until parent contains element    ${section}    xpath://img[@alt="Alt text for the uploaded annex image"]

    ${section}=    user opens accordion section    Methodology annex section 2
    ...    ${METHODOLOGY_ANNEXES_READONLY_ACCORDION}
    user verifies accordion is open    Methodology annex section 2
    user waits until element contains    ${section}    Adding Methodology annex 2 text block 1
    user waits until element contains    ${section}    Adding Methodology annex 2 text block 2
    user waits until parent contains element    ${section}
    ...    xpath://img[@alt="Alt text for the uploaded annex image 2"]
    user waits until parent contains element    ${section}
    ...    xpath://img[@alt="Alt text for the uploaded annex image 3"]

user verifies amended Methodology readonly content
    ${section}=    user opens accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_READONLY_ACCORDION}
    user waits until element contains    ${section}    Adding Methodology content    %{WAIT_SMALL}
    user waits until parent contains element    ${section}
    ...    xpath://img[@alt="Alt text for the uploaded content image"]
    user waits until parent contains element    ${section}
    ...    xpath://img[@alt="Alt text for the uploaded content image 2"]

    ${section}=    user opens accordion section    Methodology annex section 2
    ...    ${METHODOLOGY_ANNEXES_READONLY_ACCORDION}
    user verifies accordion is open    Methodology annex section 2
    user waits until element contains    ${section}    Adding Methodology annex 2 text block 2
