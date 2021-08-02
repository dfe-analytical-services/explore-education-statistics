*** Settings ***
Resource    ../common.robot
Resource    ../admin-common.robot
Library     ../admin-utilities.py

*** Variables ***
# The more complex "EDITABLE_ACCORDION" selectors below are necessary to target controls that lie outside of the
# accordion section itself e.g. the "Add new section" button.    These are necessary to use to differentiate
# between different "Add new section" buttons (as an example) when more than one set of content accordions appears
# on the page, for instance with Methodologies where Content and Annex areas are available.
${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}=      xpath://div[div[div[@id="methodologyAccordion-content"]]]
${METHODOLOGY_ANNEXES_EDITABLE_ACCORDION}=      xpath://div[div[div[@id="methodologyAccordion-annexes"]]]
${METHODOLOGY_CONTENT_READONLY_ACCORDION}=      id:methodologyAccordion-content
${METHODOLOGY_ANNEXES_READONLY_ACCORDION}=      id:methodologyAccordion-annexes

*** Keywords ***
user creates new content section
    [Arguments]
    ...    ${section_number}
    ...    ${content_section_name}
    ...    ${parent}=css:body

    user clicks element    xpath://button[.="Add new section"]    ${parent}
    user changes accordion section title    ${section_number}    ${content_section_name}    ${parent}

user opens nth editable accordion section
    [Arguments]
    ...    ${section_num}
    ...    ${parent}=css:body

    user waits until parent contains element    ${parent}
    ...    xpath:.//*[@data-testid="editableAccordionSection"][${section_num}]
    ${section}=    get child element    ${parent}
    ...    xpath:.//*[@data-testid="editableAccordionSection"][${section_num}]
    ${header_button}=    get child element    ${section}    css:h2 > button[aria-expanded]
    ${is_expanded}=    get element attribute    ${header_button}    aria-expanded
    IF    '${is_expanded}' != 'true'
        user clicks element    ${header_button}
    END
    user checks element attribute value should be    ${header_button}    aria-expanded    true

user changes accordion section title
    [Arguments]
    ...    ${section_num}
    ...    ${title}
    ...    ${parent}=id:releaseMainContent

    user opens nth editable accordion section    ${section_num}    ${parent}
    ${section}=    get child element    ${parent}
    ...    xpath:.//*[@data-testid="editableAccordionSection"][${section_num}]
    user clicks button    Edit section title    ${section}
    user waits until parent contains element    ${section}    css:input[name="heading"]
    ${input}=    get child element    ${section}    css:input[name="heading"]
    user enters text into element    ${input}    ${title}
    user clicks button    Save section title    ${section}
    user waits until parent contains element    ${section}    xpath:.//h2/button[@aria-expanded and text()="${title}"]

user checks accordion section contains x blocks
    [Arguments]
    ...    ${section_name}
    ...    ${num_blocks}
    ...    ${parent}=css:[data-testid="accordion"]

    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    ${blocks}=    get child elements    ${section}    css:[data-testid="editableSectionBlock"]
    length should be    ${blocks}    ${num_blocks}

user adds text block to editable accordion section
    [Arguments]
    ...    ${section_name}
    ...    ${parent}=css:[data-testid="accordion"]

    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    user clicks button    Add text block    ${section}
    user waits until element contains    ${section}    This section is empty

user adds data block to editable accordion section
    [Arguments]
    ...    ${section_name}
    ...    ${block_name}
    ...    ${parent}=css:[data-testid="accordion"]

    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    user waits for page to finish loading
    user clicks button    Add data block    ${section}
    ${block_list}=    get child element    ${section}    css:select[name="selectedDataBlock"]
    user chooses select option    ${block_list}    Dates data block name
    user waits until parent contains element    ${section}    css:table
    user clicks button    Embed    ${section}

user edits accordion section text block
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    get accordion section text block    ${section_name}    ${block_num}    ${parent}
    user clicks button    Edit block    ${block}
    [Return]    ${block}

user adds content to accordion section text block
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${content}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    user edits accordion section text block    ${section_name}    ${block_num}    ${parent}
    user presses keys    CTRL+a
    user presses keys    BACKSPACE
    user presses keys    ${content}
    user clicks button    Save    ${block}
    user waits until element contains    ${block}    ${content}

user adds image to accordion section text block
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${filename}=test-infographic.png
    ...    ${alt_text}=Alt text for ${filename}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    user edits accordion section text block    ${section_name}    ${block_num}    ${parent}

    choose file
    ...    xpath://button[span[.="Insert image"]]/following-sibling::input[@type="file"]
    ...    ${FILES_DIR}${filename}
    user clicks element    xpath://button[span[.="Change image text alternative"]]
    user enters text into element    label:Text alternative    ${alt_text}
    user clicks element    css:button.ck-button-save
    user clicks element    xpath://div[@title="Insert paragraph after block"]

    # wait for the API to save the image and for the src attribute to be updated before continuing
    user waits until parent contains element    ${block}    xpath://img[starts-with(@src, "/api/") and @alt="${alt_text}"]

    # Workaround to remove the eager "All images must have alternative (alt) text" validation error that persists
    # even after setting the alt text
    user presses keys    TAB
    user presses keys    SHIFT+TAB

    user clicks button    Save    ${block}

user removes image from accordion section text block
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${alt_text}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    user edits accordion section text block    ${section_name}    ${block_num}    ${parent}
    user waits until parent contains element    ${block}    xpath://img[@alt="${alt_text}"]

    # Currently assumes that the image is the first element in the text block.    Wait for the image to take focus,
    # at which point the hover controls will appear on the page (one of these being the "Change image text alternative"
    # button.    At this point, we simply press Delete and wait until the image disappears.
    user waits until element is visible    xpath://button[span[.="Change image text alternative"]]
    user presses keys    DELETE
    user waits until parent does not contain element    ${block}    xpath://img[@alt="${alt_text}"]

    # Delete the empty line left by the deleted image.
    user presses keys    DELETE
    user clicks button    Save    ${block}

user checks accordion section text block contains
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${content}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    get accordion section text block    ${section_name}    ${block_num}    ${parent}
    user waits until element contains    ${block}    ${content}

user checks accordion section text block contains image with alt text
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${alt_text}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    get accordion section text block    ${section_name}    ${block_num}    ${parent}
    user waits until parent contains element    ${block}    xpath://img[@alt="${alt_text}"]

user checks accordion section text block does not contain image with alt text
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${alt_text}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    get accordion section text block    ${section_name}    ${block_num}    ${parent}
    user waits until parent does not contain element    ${block}    xpath://img[@alt="${alt_text}"]

user deletes editable accordion section content block
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    get accordion section text block    ${section_name}    ${block_num}    ${parent}
    user clicks button    Remove block    ${block}
    user clicks button    Confirm

user deletes editable accordion section
    [Arguments]
    ...    ${section_name}
    ...    ${parent}=[data-testid="accordion"]

    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    user clicks button    Remove this section    ${section}
    user waits until modal is visible    Are you sure?
    user clicks button    Confirm

get accordion section text block
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${parent}=[data-testid="accordion"]

    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    ${block}=    get child element    ${section}    css:[data-testid="editableSectionBlock"]:nth-of-type(${block_num})
    [Return]    ${block}
