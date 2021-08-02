*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

*** Variables ***
${PUBLICATION_NAME}=    UI tests - create methodology amendment publication %{RUN_IDENTIFIER}

*** Test Cases ***
Create publicly accessible Publication
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    AY    2021
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    Academic Year 2021/22 (not Live)
    user approves release for immediate publication

Create Methodology with some content and images
    [Tags]    HappyPath

    user creates methodology for publication    ${PUBLICATION_NAME}

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
    [Tags]    HappyPath
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
    [Tags]    HappyPath
    user approves methodology for publication    ${PUBLICATION_NAME}

Verify the readonly content for the original Methodology is as expected
    [Tags]    HappyPath
    user clicks link    Manage content
    user verifies original Methodology readonly content

Create a Methodology Amendment
    [Tags]    HappyPath
    user creates methodology amendment for publication    ${PUBLICATION_NAME}
    user checks page contains tag    Draft
    user checks page contains tag    Amendment

Remove a Content Section from the Amendment
    [Tags]    HappyPath
    user clicks link    Manage content
    user opens accordion section    Methodology annex section 1    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user deletes editable accordion section    Methodology annex section 1    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

Remove an image from a Content Block
    [Tags]    HappyPath
    user opens accordion section    Methodology annex section 2    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user removes image from accordion section text block    Methodology annex section 2    2
    ...    Alt text for the uploaded annex image 3    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user checks accordion section text block does not contain image with alt text    Methodology annex section 2    2
    ...    Alt text for the uploaded content image    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

Remove a Content Block from a Content Section
    [Tags]    HappyPath
    user deletes editable accordion section content block    Methodology annex section 2    1
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}
    user checks accordion section contains x blocks    Methodology annex section 2    1
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

Add a new Content Block to an existing Content Section and include a new image
    [Tags]    HappyPath
    user opens accordion section    Methodology content section 1    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds image to accordion section text block    Methodology content section 1    2    gov-uk.png
    ...    Alt text for the uploaded content image 2    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user checks accordion section contains x blocks    Methodology content section 1    2
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

Verify all is as expected on the Amendment after the Amendment content changes
    [Tags]    HappyPath
    user checks accordion section contains x blocks    Methodology content section 1    2
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user checks accordion section text block contains image with alt text    Methodology content section 1    1
    ...    Alt text for the uploaded content image    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user checks accordion section text block contains image with alt text    Methodology content section 1    2
    ...    Alt text for the uploaded content image 2    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user checks accordion section contains x blocks    Methodology annex section 2    1
    ...    ${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}

Revisit the original Methodology and check that its content remains unaffected by the changes to the Amendment
    [Tags]    HappyPath
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user opens details dropdown    ${PUBLICATION_NAME}    ${accordion}
    user clicks link    View original methodology    ${accordion}
    user clicks link    Manage content
    user verifies original Methodology readonly content

Approve the Methodology Amendment
    [Tags]    HappyPath
    user approves methodology amendment for publication    ${PUBLICATION_NAME}

Revisit the Publication on the dashboard and check that the new Amendment is now the live Methodology
    [Tags]    HappyPath
    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user opens details dropdown    ${PUBLICATION_NAME}    ${accordion}
    user checks element contains link    ${accordion}    View this methodology
    user checks element does not contain link    ${accordion}    Edit this methodology
    user checks element contains button    ${accordion}    Amend methodology
    user checks element does not contain link    ${accordion}    Edit this amendment

Visit the approved Amendment and check that its readonly content is as expected
    [Tags]    HappyPath
    user clicks link    View this methodology
    user clicks link    Manage content
    user verifies amended Methodology readonly content

Create and cancel an Amendment
    [Tags]    HappyPath
    user creates methodology amendment for publication    ${PUBLICATION_NAME}
    user cancels methodology amendment for publication    ${PUBLICATION_NAME}

    ${accordion}=    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user opens details dropdown    ${PUBLICATION_NAME}    ${accordion}
    user checks element contains link    ${accordion}    View this methodology
    user checks element does not contain link    ${accordion}    Edit this methodology
    user checks element contains button    ${accordion}    Amend methodology
    user checks element does not contain link    ${accordion}    Edit this amendment

Revisit the live Amendment after the cancellation to double check it remains unaffected
    [Tags]    HappyPath
    user clicks link    View this methodology
    user clicks link    Manage content
    user verifies amended Methodology readonly content

*** Keywords ***
user verifies original Methodology readonly content
    ${section}=    user opens accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_READONLY_ACCORDION}
    user waits until element contains    ${section}    Adding Methodology content
    user waits until parent contains element    ${section}
    ...    xpath://img[@alt="Alt text for the uploaded content image"]

    ${section}=    user opens accordion section    Methodology annex section 1
    ...    ${METHODOLOGY_ANNEXES_READONLY_ACCORDION}
    user waits until element contains    ${section}    Adding Methodology annex
    user waits until parent contains element    ${section}    xpath://img[@alt="Alt text for the uploaded annex image"]

    ${section}=    user opens accordion section    Methodology annex section 2
    ...    ${METHODOLOGY_ANNEXES_READONLY_ACCORDION}
    user waits until element contains    ${section}    Adding Methodology annex 2 text block 1
    user waits until element contains    ${section}    Adding Methodology annex 2 text block 2
    user waits until parent contains element    ${section}
    ...    xpath://img[@alt="Alt text for the uploaded annex image 2"]
    user waits until parent contains element    ${section}
    ...    xpath://img[@alt="Alt text for the uploaded annex image 3"]

user verifies amended Methodology readonly content
    ${section}=    user opens accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_READONLY_ACCORDION}
    user waits until element contains    ${section}    Adding Methodology content
    user waits until parent contains element    ${section}
    ...    xpath://img[@alt="Alt text for the uploaded content image"]
    user waits until parent contains element    ${section}
    ...    xpath://img[@alt="Alt text for the uploaded content image 2"]

    ${section}=    user opens accordion section    Methodology annex section 2
    ...    ${METHODOLOGY_ANNEXES_READONLY_ACCORDION}
    user waits until element contains    ${section}    Adding Methodology annex 2 text block 2
