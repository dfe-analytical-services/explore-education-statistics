import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import React from 'react';

interface Props {
  contentType?: string;
}

const PrototypeModalContent = ({ contentType }: Props) => {
  const draftTag = '<Tag>Draft</Tag>';
  const approvedTag = '<Tag>Approved</Tag>';
  const inReviewTag = '<Tag>In review</Tag>';
  const amendmentTag = '<Tag>Amendment</Tag>';
  const errorTag = '<Tag colour="red">Error</Tag>';
  const warningTag = '<Tag colour="orange">Warning</Tag>';
  const commentTag = '<Tag colour="grey">Unresolved comments</Tag>';
  const validatingTag = '<Tag colour="orange">Validating</Tag>';
  const scheduledTag = '<Tag colour="blue">Scheduled</Tag>';
  const failedTag = '<Tag colour="red">Failed ✖</Tag>';
  const startedTag = '<Tag colour="orange">Started</Tag>';
  const notStartedTag = '<Tag colour="blue">Not started</Tag>';
  const completeTag = '<Tag colour="green">Complete ✓</Tag>';
  const cancelledTag = '<Tag colour="red">PUBLISHING CANCELLED ✖</Tag>';
  const publishedTag = '<Tag colour="green">Published</Tag>';

  return (
    <>
      {contentType === 'helpStatusModal' && (
        <>
          <p>
            These are releases that can be edited prior to publication. The
            different types of draft release status are described below.
          </p>
          <SummaryList>
            <SummaryListItem term={draftTag}>
              This is an unpublished draft release
            </SummaryListItem>
            <SummaryListItem term={inReviewTag}>
              This is a release that is ready to be reviewed prior to
              publication
            </SummaryListItem>
            <SummaryListItem term={amendmentTag}>
              This is a published release that is currently being amended
            </SummaryListItem>
          </SummaryList>
        </>
      )}
      {contentType === 'helpIssuesModal' && (
        <>
          <p>
            This is a summary of issues that could be associated to a draft
            release. Click the 'View issues' link to reveal the details. The
            different types of issues are described below:
          </p>
          <SummaryList>
            <SummaryListItem term={errorTag}>
              These are issues that need to be resolved before publication
            </SummaryListItem>
            <SummaryListItem term={warningTag}>
              These are things you may have forgotten, but do not need to
              resolve to publish the release
            </SummaryListItem>
            <SummaryListItem term={commentTag}>
              These are unresolved comments that you may wish to check before
              publishing the release
            </SummaryListItem>
          </SummaryList>
        </>
      )}
      {contentType === 'releaseIssuesModal' && (
        <>
          <p>
            This is a summary of issues that could be associated to a draft
            release.
          </p>
          <SummaryList>
            <SummaryListItem term="Errors">
              These are issues that need to be resolved before publication
            </SummaryListItem>
            <SummaryListItem term="Warnings">
              These are things you may have forgotten, but do not need to
              resolve to publish the release
            </SummaryListItem>
            <SummaryListItem term="Unresolved comments">
              These are unresolved comments that you may wish to check before
              publishing the release
            </SummaryListItem>
          </SummaryList>
        </>
      )}
      {contentType === 'helpPublishedModal' && (
        <SummaryList>
          <SummaryListItem term={publishedTag}>
            This is a published release that is current live and is availble for
            public view. If you need to make any changes to the text or data in
            a live a release you can select the 'Amend' option, this will allow
            you to re-publish the chosen release with the new amendments
          </SummaryListItem>
        </SummaryList>
      )}
      {contentType === 'scheduledStatusModal' && (
        <>
          <p>
            This is a summary of the different status types associated to
            approved scheduled releases.
          </p>
          <div style={{ overflow: 'auto', maxHeight: '70vh' }}>
            <SummaryList>
              <SummaryListItem term={validatingTag}>
                This is a release that has just been approved. The system is
                validating that everything is OK for publication
              </SummaryListItem>
              <SummaryListItem term={scheduledTag}>
                This is a release that has been approved and validated and is
                now scheduled for release on the 'Scheduled publish date'
              </SummaryListItem>
              <SummaryListItem term={startedTag}>
                The publication process has now started
              </SummaryListItem>
              <SummaryListItem term={completeTag}>
                The release has now been successfully published and is now live
                for public view
              </SummaryListItem>
              <SummaryListItem term={failedTag}>
                There is a problem preventing the release from being
                successfully published. Contact{' '}
                <a href="mailto:explore.statistics@education.gov.uk">
                  explore.statistics@education.gov.uk
                </a>{' '}
                for further assistance
              </SummaryListItem>
              <SummaryListItem term="View stages">
                For <Tag colour="red">Failed</Tag>,{' '}
                <Tag colour="orange">Started</Tag> or{' '}
                <Tag colour="green">Complete</Tag> releases clicking 'View
                stages' will show a summary of the publication status,
                highlighting all the publication steps and whether they have
                succeeded or failed
              </SummaryListItem>
            </SummaryList>
          </div>
        </>
      )}
      {contentType === 'scheduledStagesModal' && (
        <>
          <p>
            For <Tag colour="red">Failed</Tag>,{' '}
            <Tag colour="orange">Started</Tag> or{' '}
            <Tag colour="green">Complete</Tag> releases clicking 'View stages'
            will show a summary of the publication status. There are 4 steps of
            the process (Data, content, files and final publishing). For each of
            these steps there are different stages as described below:
          </p>
          <SummaryList>
            <SummaryListItem term={notStartedTag}>
              This step has yet to start publishing
            </SummaryListItem>
            <SummaryListItem term={startedTag}>
              This step has now started the publication process
            </SummaryListItem>
            <SummaryListItem term={completeTag}>
              This step of the process has successfully completed
            </SummaryListItem>
            <SummaryListItem term={failedTag}>
              This step of the process has failed, and the publication process
              will be cancelled
            </SummaryListItem>
            <SummaryListItem term={cancelledTag}>
              There is a problem preventing the release from being successfully
              published. Contact{' '}
              <a href="mailto:explore.statistics@education.gov.uk">
                explore.statistics@education.gov.uk
              </a>{' '}
              for further assistance
            </SummaryListItem>
          </SummaryList>
        </>
      )}
      {contentType === 'methodologyStatusModal' && (
        <>
          <p>
            A methodology can have various types of status as descibed below:
          </p>
          <SummaryList>
            <SummaryListItem term={draftTag}>
              This is an unpublished draft methodology that can still be edited
            </SummaryListItem>
            <SummaryListItem term={approvedTag}>
              This is an approved methodology that can be published at the same
              as a specific release
            </SummaryListItem>
            <SummaryListItem term={publishedTag}>
              This is a published methodology that is live for public view
            </SummaryListItem>
          </SummaryList>
        </>
      )}
      {contentType === 'helpReleaseTypes' && (
        <>
          <p>
            This is a description list of the different types of publication we
            currently publish.
          </p>
          <div
            style={{
              maxHeight: '50vh',
              overflowY: 'auto',
              marginBottom: '2rem',
            }}
          >
            <SummaryList>
              <SummaryListItem term="Ad hoc statistics">
                Releases of statistics which are not part of DfE's regular
                annual official statistical release calendar.
              </SummaryListItem>
              <SummaryListItem term="Experimental statistics">
                Experimental statistics are newly developed or innovative
                official statistics that are undergoing evaluation. They are
                published to involve users and stakeholders in the assessment of
                their suitability and quality at an early stage. These
                statistics will reach a point where the label, experimental
                statistics, can be removed, or should be discontinued.
              </SummaryListItem>
              <SummaryListItem term="Management information (MI)">
                Management information describes aggregate information collated
                and used in the normal course of business to inform operational
                delivery, policy development or the management of organisational
                performance. It is usually based on administrative data but can
                also be a product of survey data. The terms administrative data
                and management information are sometimes used interchangeably.
              </SummaryListItem>
              <SummaryListItem term="National statistics">
                National statistics are official statistics that have been
                assessed by the Office for Statistics Regulation as fully
                compliant with the Code of Practice for Statistics, i.e., they
                meet the highest standards for trustworthiness, quality and
                value.
              </SummaryListItem>
              <SummaryListItem term="Official statistics">
                Official statistics are regular statistics produced by the UK
                Statistics Authority, government departments (including
                executive agencies), the Devolved Administrations in Scotland,
                Wales and Northern Ireland, any other person acting on behalf of
                the Crown or any other organisation named on an Official
                Statistics Order.
              </SummaryListItem>
            </SummaryList>
          </div>
        </>
      )}
      {contentType === 'helpThemes' && (
        <>
          <p>This is a description list of our different publication themes.</p>
          <div
            style={{
              maxHeight: '50vh',
              overflowY: 'auto',
              marginBottom: '2rem',
            }}
          >
            <SummaryList>
              <SummaryListItem term="Early years (pre school)">
                Early years foundation stage profile and early years surveys
                statistics
              </SummaryListItem>
              <SummaryListItem term="Pupils and schools">
                Absence, application and offers, capacity, exclusion and special
                educational needs (SEN) statistics
              </SummaryListItem>
              <SummaryListItem term="Teachers, workforce and school funding">
                Initial teacher training (ITT) statistics, local authority (LA)
                and student loan statistics
              </SummaryListItem>
              <SummaryListItem term="Education outcomes and performance">
                Includes not in education, employment or training (NEET)
                statistics
              </SummaryListItem>
              <SummaryListItem term="Further education">
                Advanced learner loan, benefit claimant and apprenticeship and
                traineeship statistics
              </SummaryListItem>
              <SummaryListItem term="Higher education">
                University graduate employment, graduate labour market and
                participation statistics
              </SummaryListItem>
              <SummaryListItem term="Children's social care">
                Children in need and child protection, children looked after and
                social work workforce statistics
              </SummaryListItem>
              <SummaryListItem term="COVID-19">
                Attendance in education and early years settings during the
                coronavirus (COVID-19) outbreak
              </SummaryListItem>
              <SummaryListItem term="Cross-cutting publications">
                Summarised expenditure, post-compulsory education, qualification
                and school statistics
              </SummaryListItem>
            </SummaryList>
          </div>
        </>
      )}
    </>
  );
};

export default PrototypeModalContent;
