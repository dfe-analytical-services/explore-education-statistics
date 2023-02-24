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
user adds basic release content
    [Arguments]    ${publication}
    user adds summary text block
    user adds content to summary text block    Test summary text for ${publication}

    user adds headlines text block
    user adds content to headlines text block    Test headlines summary text for ${publication}

    user waits until button is enabled    Add new section
    user clicks button    Add new section

    user changes accordion section title    1    Test section one    id:releaseMainContent
    user adds text block to editable accordion section    Test section one    id:releaseMainContent
    user adds content to autosaving accordion section text block    Test section one    1
    ...    Test content block for ${publication}    id:releaseMainContent

user creates new content section
    [Arguments]
    ...    ${section_number}
    ...    ${content_section_name}
    ...    ${parent}=css:body

    user clicks element    xpath://button[.="Add new section"]    ${parent}
    user changes accordion section title    ${section_number}    ${content_section_name}    ${parent}

user creates data block for dates csv
    [Arguments]
    ...    ${subject_name}
    ...    ${datablock_name}
    ...    ${datablock_title}

    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

    user clicks link    Create data block
    user waits until h2 is visible    Create data block

    user waits until table tool wizard step is available    1    Choose a subject
    user waits until page contains    ${subject_name}
    user clicks radio    ${subject_name}
    user clicks element    id:publicationSubjectForm-submit

    user waits until table tool wizard step is available    2    Choose locations
    user opens details dropdown    National
    user checks location checkbox is checked    England

    user clicks element    id:locationFiltersForm-submit

    user waits until table tool wizard step is available    3    Choose time period
    user chooses select option    id:timePeriodForm-start    2020 Week 13
    user chooses select option    id:timePeriodForm-end    2020 Week 16
    user clicks element    id:timePeriodForm-submit

    user waits until table tool wizard step is available    4    Choose your filters
    user clicks subheaded indicator checkbox    Open settings    Number of open settings
    user checks subheaded indicator checkbox is checked    Open settings    Number of open settings
    user clicks subheaded indicator checkbox    Open settings    Proportion of settings open
    user checks subheaded indicator checkbox is checked    Open settings    Proportion of settings open

    user opens details dropdown    Date
    user clicks category checkbox    Date    23/03/2020
    user checks category checkbox is checked    Date    23/03/2020

    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_LONG}

    user checks table column heading contains    1    1    2020 Week 13
    user checks headed table body row cell contains    Number of open settings    1    22,900
    user checks headed table body row cell contains    Proportion of settings open    1    1%

    user enters text into element    id:dataBlockDetailsForm-name    ${datablock_name}
    user enters text into element    id:dataBlockDetailsForm-heading    ${datablock_title}
    user enters text into element    id:dataBlockDetailsForm-source    Dates source

    user clicks button    Save data block

    user waits until h2 is visible    Edit data block
    user waits until page contains button    Delete this data block

user creates key stats data block for dates csv
    [Arguments]
    ...    ${subject_name}
    ...    ${datablock_name}
    ...    ${datablock_title}
    ...    ${indicator_name}
    ...    ${expected_indicator_value}

    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

    user clicks link    Create data block
    user waits until h2 is visible    Create data block

    user waits until table tool wizard step is available    1    Choose a subject
    user waits until page contains    ${subject_name}
    user clicks radio    ${subject_name}
    user clicks element    id:publicationSubjectForm-submit

    user waits until table tool wizard step is available    2    Choose locations
    user opens details dropdown    National
    user checks location checkbox is checked    England

    user clicks element    id:locationFiltersForm-submit

    user waits until table tool wizard step is available    3    Choose time period
    user chooses select option    id:timePeriodForm-start    2020 Week 13
    user chooses select option    id:timePeriodForm-end    2020 Week 13
    user clicks element    id:timePeriodForm-submit

    user waits until table tool wizard step is available    4    Choose your filters
    user clicks subheaded indicator checkbox    Open settings    ${indicator_name}
    user checks subheaded indicator checkbox is checked    Open settings    ${indicator_name}

    user opens details dropdown    Date
    user clicks category checkbox    Date    23/03/2020
    user checks category checkbox is checked    Date    23/03/2020

    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_LONG}

    user checks table column heading contains    1    1    2020 Week 13
    user checks headed table body row cell contains    ${indicator_name}    1    ${expected_indicator_value}

    user enters text into element    id:dataBlockDetailsForm-name    ${datablock_name}
    user enters text into element    id:dataBlockDetailsForm-heading    ${datablock_title}
    user enters text into element    id:dataBlockDetailsForm-source    Dates source

    user clicks button    Save data block

    user waits until h2 is visible    Edit data block
    user waits until page contains button    Delete this data block

user chooses and embeds data block
    [Arguments]
    ...    ${datablock_name}
    user chooses select option    css:select[name="selectedDataBlock"]    ${datablock_name}
    user waits until button is enabled    Embed    %{WAIT_SMALL}
    user clicks button    Embed
    user waits until page does not contain button    Embed    %{WAIT_MEDIUM}
    user waits until page does not contain loading spinner

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

user adds summary text block
    user clicks button    Add a summary text block    id:releaseSummary
    user waits until element contains    id:releaseSummary    This section is empty    %{WAIT_SMALL}

user adds headlines text block
    user clicks button    Add a headlines text block    id:releaseHeadlines
    user waits until element contains    id:releaseHeadlines    This section is empty    %{WAIT_SMALL}

user adds text block to editable accordion section
    [Arguments]
    ...    ${section_name}
    ...    ${parent}=css:[data-testid="accordion"]

    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    user clicks button    Add text block    ${section}
    user waits until element contains    ${section}    This section is empty    %{WAIT_SMALL}

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
    user waits until parent does not contain element    ${section}    xpath://button[text()="Embed"]

user chooses to embed a URL in editable accordion section
    [Arguments]
    ...    ${section_name}
    ...    ${section_parent}=css:[data-testid="accordion"]

    user opens accordion section    ${section_name}    ${section_parent}
    ${section}=    user gets accordion section content element    ${section_name}    ${section_parent}
    user clicks button    Embed a URL    ${section}

user chooses to update an embedded URL in editable accordion section
    [Arguments]
    ...    ${section_name}
    ...    ${section_parent}=css:[data-testid="accordion"]

    user opens accordion section    ${section_name}    ${section_parent}
    ${section}=    user gets accordion section content element    ${section_name}    ${section_parent}
    user clicks button    Edit embedded URL    ${section}

user updates embedded URL details in modal
    [Arguments]
    ...    ${title}
    ...    ${url}
    ...    ${modal_heading}=Embed a URL

    ${modal}=    user waits until modal is visible    ${modal_heading}

    ${title_input}=    get child element    ${modal}    id:embedBlockForm-title
    user enters text into element    ${title_input}    ${title}

    ${url_input}=    get child element    ${modal}    id:embedBlockForm-url
    user enters text into element    ${url_input}    ${url}
    [Return]    ${modal}

user starts editing accordion section text block
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    get accordion section block    ${section_name}    ${block_num}    ${parent}
    user starts editing text block    ${block}
    [Return]    ${block}

user starts editing text block
    [Arguments]    ${parent}
    user clicks button    Edit block    ${parent}
    user waits until parent does not contain button    ${parent}    Edit block
    user waits until parent contains element    ${parent}    css:[role="textbox"]
    ${element}=    get child element    ${parent}    css:[role="textbox"]
    user waits until element is enabled    ${element}

user adds content to autosaving text block
    [Arguments]    ${parent}    ${content}    ${save}=True
    user starts editing text block    ${parent}
    user presses keys    CTRL+a
    user presses keys    BACKSPACE
    user presses keys    ${content}
    IF    ${save}
        user saves autosaving text block    ${parent}
    END
    user waits until element contains    ${parent}    ${content}    %{WAIT_SMALL}

user adds content to summary text block
    [Arguments]    ${content}
    user adds content to autosaving text block    id:releaseSummary    ${content}

user adds content to headlines text block
    [Arguments]    ${content}
    user adds content to autosaving text block    id:releaseHeadlines    ${content}

user adds content to accordion section text block
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${content}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    user starts editing accordion section text block    ${section_name}    ${block_num}    ${parent}
    user presses keys    CTRL+a
    user presses keys    BACKSPACE
    user presses keys    ${content}
    user clicks button    Save    ${block}
    user waits until element contains    ${block}    ${content}    %{WAIT_SMALL}

user adds content to autosaving accordion section text block
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${content}
    ...    ${parent}=[data-testid="accordion"]
    ...    ${save}=True

    ${block}=    get accordion section block    ${section_name}    ${block_num}    ${parent}
    user adds content to autosaving text block    ${block}    ${content}    ${save}

user adds image to accordion section text block
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${filename}=test-infographic.png
    ...    ${alt_text}=Alt text for ${filename}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    user starts editing accordion section text block    ${section_name}    ${block_num}    ${parent}

    # If we don't do this, `Insert paragraph after block` circle button on image doesn't appear
    user presses keys    ${\n}
    user presses keys    ARROW_UP

    choose file
    ...    xpath://button[span[.="Insert image"]]/following-sibling::input[@type="file"]
    ...    ${FILES_DIR}${filename}
    user clicks element    xpath://button[span[.="Change image text alternative"]]
    user enters text into element    label:Text alternative    ${alt_text}
    user clicks element    css:button.ck-button-save
    user clicks element    xpath://div[@title="Insert paragraph after block"]

    # wait for the API to save the image and for the src attribute to be updated before continuing
    user waits until parent contains element    ${block}
    ...    xpath://img[starts-with(@src, "/api/") and @alt="${alt_text}"]

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
    ...    ${save_button}=Save & close

    ${block}=    user starts editing accordion section text block    ${section_name}    ${block_num}    ${parent}
    user waits until parent contains element    ${block}    xpath://img[@alt="${alt_text}"]

    # Currently assumes that the image is the first element in the text block.    Wait for the image to take focus,
    # at which point the hover controls will appear on the page (one of these being the "Change image text alternative"
    # button.    At this point, we simply press Delete and wait until the image disappears.
    user waits until element is visible    xpath://button[span[.="Change image text alternative"]]
    user presses keys    DELETE
    user waits until parent does not contain element    ${block}    xpath://img[@alt="${alt_text}"]

    # Delete the empty line left by the deleted image.
    user presses keys    DELETE
    user clicks button    ${save_button}    ${block}

user saves autosaving text block
    [Arguments]    ${parent}
    user checks element contains button    ${parent}    Save & close

    # EES-3501 - moving focus out of the autosave textarea to give the onBlur() with the 100ms timeout in
    # FormEditor.tsx a chance to process prior to processing the form submission when we click "Save & close".
    user presses keys    TAB
    sleep    0.2

    user clicks button    Save & close    ${parent}
    user waits until parent does not contain button    ${parent}    Save & close    %{WAIT_SMALL}

user checks accordion section text block contains
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${content}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    get accordion section block    ${section_name}    ${block_num}    ${parent}
    user waits until element contains    ${block}    ${content}

user checks accordion section text block contains image with alt text
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${alt_text}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    get accordion section block    ${section_name}    ${block_num}    ${parent}
    user waits until parent contains element    ${block}    xpath://img[@alt="${alt_text}"]

user checks accordion section text block does not contain image with alt text
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${alt_text}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    get accordion section block    ${section_name}    ${block_num}    ${parent}
    user waits until parent does not contain element    ${block}    xpath://img[@alt="${alt_text}"]

user deletes editable accordion section content block
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${parent}=[data-testid="accordion"]

    ${block}=    get accordion section block    ${section_name}    ${block_num}    ${parent}
    user clicks button    Remove block    ${block}
    user clicks button    Confirm
    # avoid blocks being lazy loaded
    user waits until page does not contain loading spinner
    Sleep    0.75
    user scrolls down    1

user deletes editable accordion section
    [Arguments]
    ...    ${section_name}
    ...    ${parent}=[data-testid="accordion"]

    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    user clicks button    Remove this section    ${section}
    user waits until modal is visible    Removing section
    user clicks button    Confirm
    user waits until modal is not visible    Removing section

get accordion section block
    [Arguments]
    ...    ${section_name}
    ...    ${block_num}
    ...    ${parent}=[data-testid="accordion"]

    ${section}=    user gets accordion section content element    ${section_name}    ${parent}
    ${block}=    get child element    ${section}    css:[data-testid="editableSectionBlock"]:nth-of-type(${block_num})
    [Return]    ${block}

get editor toolbar
    [Arguments]    ${block}
    ${toolbar}=    lookup or return webelement    css:[aria-label="Editor toolbar"]    ${block}
    [Return]    ${toolbar}

get editor
    [Arguments]    ${block}
    ${editor}=    lookup or return webelement    css:[aria-label="Rich Text Editor, main"]    ${block}
    [Return]    ${editor}

get comments sidebar
    [Arguments]    ${block}
    ${comments}=    lookup or return webelement    testid:comments-sidebar    ${block}
    [Return]    ${comments}
