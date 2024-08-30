import { InvalidContentError } from '@admin/components/editable/utils/getInvalidContent';
import Modal from '@common/components/Modal';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import uniqBy from 'lodash/uniqBy';
import React, { ReactNode } from 'react';

export default function InvalidContentDetails({
  errors,
}: {
  errors: InvalidContentError[];
}) {
  const clickHereLinkTextErrors = errors.filter(
    error => error.type === 'clickHereLinkText',
  );
  const repeatedLinkTextErrors = errors.filter(
    error => error.type === 'repeatedLinkText',
  );
  const filteredRepeatedLinkTextErrors = uniqBy(
    repeatedLinkTextErrors,
    'message',
  );
  const oneWordLinkTextErrors = errors.filter(
    error => error.type === 'oneWordLinkText',
  );
  const urlLinkTextErrors = errors.filter(
    error => error.type === 'urlLinkText',
  );
  const skippedHeadingLevelErrors = errors.filter(
    error => error.type === 'skippedHeadingLevel',
  );
  const missingTableHeadersErrors = errors.filter(
    error => error.type === 'missingTableHeaders',
  );
  const boldAsHeadingErrors = errors.filter(
    error => error.type === 'boldAsHeading',
  );
  const emptyHeadingErrors = errors.filter(
    error => error.type === 'emptyHeading',
  );

  return (
    <>
      <p>The following accessibility problems have been found:</p>
      <ul>
        {!!clickHereLinkTextErrors.length && (
          <ErrorItem
            modalContent={
              <>
                <p>
                  Links are often viewed out of context and used to help
                  navigate pages like headers, it is important that they are
                  descriptive and understandable on their own.
                </p>
                <p>
                  Avoid using "click here" as link text as it does not describe
                  where the link is to.
                </p>
              </>
            }
            modalTitle='"Click here" links'
            text={`${clickHereLinkTextErrors.length} "click here" ${
              clickHereLinkTextErrors.length === 1 ? 'link' : 'links'
            }`}
          />
        )}

        {!!repeatedLinkTextErrors.length && (
          <ErrorItem
            detailsList={filteredRepeatedLinkTextErrors.map((error, index) => (
              <li key={`error-${index.toString()}`}>
                {error.message}: {error.details}
              </li>
            ))}
            modalContent={
              <>
                <p>
                  Links are often viewed out of context and used to help
                  navigate pages like headers, it is important that they are
                  descriptive and understandable on their own.
                </p>
                <ul>
                  <li>
                    Do not use the same link text to link to different places
                    within a page.
                  </li>
                  <li>
                    Avoid linking to the same place more than once if you can,
                    if you must, use the same link text.
                  </li>
                </ul>
              </>
            }
            modalTitle="Repeated link text"
            text={`${repeatedLinkTextErrors.length} links have the same text with different URLs`}
          />
        )}

        {!!oneWordLinkTextErrors.length && (
          <ErrorItem
            detailsList={oneWordLinkTextErrors.map((error, index) => (
              <li key={`error-${index.toString()}`}>{error.message}</li>
            ))}
            modalContent={
              <>
                <p>
                  Links are often viewed out of context and used to help
                  navigate pages like headers, it is important that they are
                  descriptive and understandable on their own.
                </p>
                <p>Avoid one word links because:</p>
                <ul>
                  <li>
                    they can create problems for users with limited dexterity to
                    click on a small area
                  </li>
                  <li>
                    having a single word limits how descriptive and user
                    friendly it can be
                  </li>
                </ul>
              </>
            }
            modalTitle="One word link text"
            text={`${oneWordLinkTextErrors.length} ${
              oneWordLinkTextErrors.length === 1 ? 'link' : 'links'
            } with one word link text`}
          />
        )}

        {!!urlLinkTextErrors.length && (
          <ErrorItem
            detailsList={urlLinkTextErrors.map((error, index) => (
              <li key={`error-${index.toString()}`}>{error.message}</li>
            ))}
            modalContent={
              <>
                <p>
                  Links are often viewed out of context and used to help
                  navigate pages like headers, it is important that they are
                  descriptive and understandable on their own.
                </p>
                <p>
                  Do not use the raw URL in the text as this will be read in
                  full by screen readers and won't necessarily be descriptive
                  and understandable.
                </p>
              </>
            }
            modalTitle="URLs as link text"
            text={`${urlLinkTextErrors.length} ${
              urlLinkTextErrors.length === 1 ? 'link' : 'links'
            } with a URL as link text`}
          />
        )}

        {!!skippedHeadingLevelErrors.length && (
          <ErrorItem
            detailsList={skippedHeadingLevelErrors.map((error, index) => (
              <li key={`error-${index.toString()}`}>{error.message}</li>
            ))}
            modalContent={
              <>
                <p>
                  Heading should be structured in logical way. This means that
                  you shouldn't skip heading levels while going downwards (h1 to
                  h2 to h3…). All headings must be preceded by a level that is:
                </p>

                <ul>
                  <li>One level lower (e.g. where a h2 is followed by a h3)</li>
                  <li>Equal (e.g. multiple h3 headings in a row)</li>
                  <li>
                    A higher level (e.g. having a h3 then h4 then h5, and then
                    back up to h3)
                  </li>
                </ul>
                <p>
                  In EES you have the options of normal paragraph text and
                  headings 3, 4 and 5 and all section titles are set to heading
                  level 2. For example having a h4 as the first heading in a
                  section means that the heading order is:
                </p>
                <ul>
                  <li>h2 (section title)</li>
                  <li>h4</li>
                </ul>
                <p>
                  In this example the leap from the h2 to the h4 is unexpected,
                  and you should use a h3 instead of a h4.
                </p>
                <p>You can set the heading level in the toolbar:</p>
                <p>
                  <img
                    src="/assets/images/accessibility-guidance/setting-heading-level.png"
                    alt="Heading level setting in the toolbar"
                  />
                </p>
              </>
            }
            modalTitle="Skipped heading levels"
            text={`${skippedHeadingLevelErrors.length} skipped heading ${
              skippedHeadingLevelErrors.length === 1 ? 'level' : 'levels'
            }`}
          />
        )}

        {!!boldAsHeadingErrors.length && (
          <ErrorItem
            detailsList={boldAsHeadingErrors.map((error, index) => (
              <li key={`error-${index.toString()}`}>{error.message}</li>
            ))}
            modalContent={
              <>
                <p>
                  The correct use of headings is a critical consideration for
                  accessibility. Screen reader users have the ability to have
                  pages read out by header titles alone as this provides a way
                  to quickly understand the content of the page without having
                  to read through all of it.
                </p>
                <p>Overuse of bold text can cause confusion:</p>
                <ul>
                  <li>
                    when used instead of headings, this means the text won't be
                    recognised as a structural heading
                  </li>
                  <li>
                    using it for full sentences or paragraphs can make text more
                    difficult to read and may cause confusion with some users
                    who notice a change in format and assume it is a heading
                    that's not been marked up correctly
                  </li>
                </ul>
                <p>
                  We recommend using bold sparingly and suggest that you only
                  use it for the particular words or phrases within a sentence
                  that you'd like to highlight.
                </p>
              </>
            }
            modalTitle="Bold text as headings"
            text={`${boldAsHeadingErrors.length} ${
              boldAsHeadingErrors.length === 1 ? 'line' : 'lines'
            } with bold text used instead of a heading`}
          />
        )}

        {!!emptyHeadingErrors.length && (
          <ErrorItem
            modalContent={
              <>
                <p>
                  The correct use of headings is a critical consideration for
                  accessibility. Screen reader users have the ability to have
                  pages read out by Header titles alone, this provides a way to
                  quickly understand the content of the page without having to
                  read through all of it.
                </p>
                <p>
                  Screen readers alert users to the presence of a heading tag.
                  If the heading is empty or the text cannot be accessed, this
                  could either confuse users or even prevent them from accessing
                  information on the page's structure.
                </p>
              </>
            }
            modalTitle="Empty headings"
            text={`${emptyHeadingErrors.length} empty ${
              emptyHeadingErrors.length === 1 ? 'heading' : 'headings'
            }`}
          />
        )}

        {!!missingTableHeadersErrors.length && (
          <ErrorItem
            modalContent={
              <>
                <p>
                  All tables should have a header row or header column, styling
                  as bold is not sufficient as it doesn't declare the headers in
                  a way that assistive technology can interpret.
                </p>
                <p>
                  You can assign a row or column to be a header by clicking on a
                  cell and using the toolbar the pops up:
                </p>
                <p>
                  <img
                    src="/assets/images/accessibility-guidance/adding-table-headers.png"
                    alt="Table cell toolbar"
                  />
                </p>
              </>
            }
            modalTitle="Missing table headers"
            text={`${missingTableHeadersErrors.length} ${
              missingTableHeadersErrors.length === 1
                ? 'table has'
                : 'tables have'
            } missing headers`}
          />
        )}
      </ul>
    </>
  );
}

interface ErrorItemProps {
  detailsList?: ReactNode;
  modalContent: ReactNode;
  modalTitle: string;
  text: string;
}

function ErrorItem({
  detailsList,
  modalContent,
  modalTitle,
  text,
}: ErrorItemProps) {
  return (
    <li>
      <Modal
        showClose
        title={modalTitle}
        triggerButton={
          <ButtonText>
            {text} <InfoIcon description="Show information about this error" />
          </ButtonText>
        }
      >
        {modalContent}
      </Modal>
      <ul className="govuk-!-margin-bottom-1 govuk-!-margin-top-1">
        {detailsList}
      </ul>
    </li>
  );
}
