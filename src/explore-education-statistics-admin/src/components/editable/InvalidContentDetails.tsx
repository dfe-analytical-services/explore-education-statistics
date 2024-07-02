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
  const internalLinkOpensInSameTabErrors = errors.filter(
    error => error.type === 'internalLinkOpensInSameTab',
  );
  const externalOpensLinkInNewTabErrors = errors.filter(
    error => error.type === 'externalOpensLinkInNewTab',
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

        {!!internalLinkOpensInSameTabErrors.length && (
          <ErrorItem
            detailsList={internalLinkOpensInSameTabErrors.map(
              (error, index) => (
                <li key={`error-${index.toString()}`}>{error.message}</li>
              ),
            )}
            modalContent={
              <p>
                Internal links should open in the same tab so that users can
                navigate consistently around the site using the browser back and
                next buttons.
              </p>
            }
            modalTitle="Internal links should open in the same tab"
            text={`${internalLinkOpensInSameTabErrors.length} internal ${
              internalLinkOpensInSameTabErrors.length === 1
                ? 'link does'
                : 'links do'
            } not open in the same tab`}
          />
        )}

        {!!externalOpensLinkInNewTabErrors.length && (
          <ErrorItem
            detailsList={externalOpensLinkInNewTabErrors.map((error, index) => (
              <li key={`error-${index.toString()}`}>{error.message}</li>
            ))}
            modalContent={
              <p>
                External links should open in a new tab so that users are not
                taken away from the current website and can easily switch
                between websites.
              </p>
            }
            modalTitle="External links should open in a new tab"
            text={`${externalOpensLinkInNewTabErrors.length} external ${
              externalOpensLinkInNewTabErrors.length === 1
                ? 'link does'
                : 'links do'
            } not open in a new tab`}
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
                  Headings should be structured in logical way. In EES you have
                  the options of normal paragraph text and headings 3, 4 and 5.
                  Section titles are set to heading level 2.
                </p>

                <p>All headings must be preceded by a level that is</p>
                <ul>
                  <li>One level below (e.g. 3 logically follows 2)</li>
                  <li>Equal (e.g. multiple 3 headings in a row)</li>
                  <li>
                    Any higher level (e.g. coming back to 3 after a 4 or 5)
                  </li>
                </ul>
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
                  pages read out by Header titles alone, this provides a way to
                  quickly understand the content of the page without having to
                  read through all of it.
                </p>
                <p>
                  Do not use bold for headings as it won't be recognised as a
                  structural heading
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
