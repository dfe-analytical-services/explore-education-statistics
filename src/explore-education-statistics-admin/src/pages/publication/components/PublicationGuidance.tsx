import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import Modal from '@common/components/Modal';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import React from 'react';

export function DraftStatusGuidanceModal() {
  return (
    <Modal
      className="govuk-!-width-one-half"
      title="Draft status guidance"
      showClose
      triggerButton={
        <ButtonText>
          <InfoIcon description="Guidance on draft statuses" />
        </ButtonText>
      }
    >
      <p>
        These are releases that can be edited prior to publication. The
        different types of draft release status are described below.
      </p>
      <SummaryList>
        <SummaryListItem term={<Tag>Draft</Tag>}>
          This is an unpublished draft release
        </SummaryListItem>
        <SummaryListItem term={<Tag>In Review</Tag>}>
          This is a release that is ready to be reviewed prior to publication
        </SummaryListItem>
        <SummaryListItem term={<Tag>Draft Amendment</Tag>}>
          This is a published release that is currently being amended
        </SummaryListItem>
        <SummaryListItem term={<Tag>Draft In Review</Tag>}>
          This is a published release that has been amended and is ready to be
          reviewed prior to publication
        </SummaryListItem>
      </SummaryList>
    </Modal>
  );
}

export function IssuesGuidanceModal() {
  return (
    <Modal
      className="govuk-!-width-one-half"
      title="Issues guidance"
      showClose
      triggerButton={
        <ButtonText>
          <InfoIcon description="Guidance on draft release issues" />
        </ButtonText>
      }
    >
      <p>
        This is a summary of issues that could be associated to a draft release.
      </p>
      <SummaryList>
        <SummaryListItem term="Errors">
          These are issues that need to be resolved before publication
        </SummaryListItem>
        <SummaryListItem term="Warnings">
          These are things you may have forgotten, but do not need to resolve to
          publish the release
        </SummaryListItem>
      </SummaryList>
    </Modal>
  );
}

export function PublishedStatusGuidanceModal() {
  return (
    <Modal
      className="govuk-!-width-one-half"
      title="Published status guidance"
      showClose
      triggerButton={
        <ButtonText>
          <InfoIcon description="Guidance on statuses" />
        </ButtonText>
      }
    >
      <SummaryList>
        <SummaryListItem term={<Tag colour="green">Published</Tag>}>
          This is a published release that is current live and is available for
          public view. If you need to make any changes to the text or data in a
          published release you can select the 'Amend' option, this will allow
          you to re-publish the chosen release with the new amendments
        </SummaryListItem>
      </SummaryList>
    </Modal>
  );
}

export function ScheduledStagesGuidanceModal() {
  return (
    <Modal
      className="govuk-!-width-one-half"
      title="Publication stages guidance"
      showClose
      triggerButton={
        <ButtonText>
          <InfoIcon description="Guidance on publication stages" />
        </ButtonText>
      }
    >
      <p>
        For <Tag colour="red">Failed</Tag>, <Tag colour="orange">Started</Tag>{' '}
        or <Tag colour="green">Complete</Tag> releases clicking 'View stages'
        will show a summary of the publication status. There are 4 steps of the
        process (data, content, files and final publishing). For each of these
        steps there are different stages as described below:
      </p>
      <SummaryList>
        <SummaryListItem term={<Tag colour="blue">Not started</Tag>}>
          The release has not started the publication process
        </SummaryListItem>
        <SummaryListItem term={<Tag colour="orange">Started</Tag>}>
          This step has now started the publication process
        </SummaryListItem>
        <SummaryListItem term={<Tag colour="green">Complete ✓</Tag>}>
          This step of the process has successfully completed
        </SummaryListItem>
        <SummaryListItem term={<Tag colour="red">Failed ✖</Tag>}>
          This step of the process has failed, and the publication process will
          be cancelled
        </SummaryListItem>
        <SummaryListItem term={<Tag colour="red">Publishing cancelled ✖</Tag>}>
          There is a problem preventing the release from being successfully
          published. Contact{' '}
          <a href="mailto:explore.statistics@education.gov.uk">
            explore.statistics@education.gov.uk
          </a>{' '}
          for further assistance
        </SummaryListItem>
      </SummaryList>
    </Modal>
  );
}

export function ScheduledStatusGuidanceModal() {
  return (
    <Modal
      className="govuk-!-width-one-half"
      title="Scheduled status guidance"
      showClose
      triggerButton={
        <ButtonText>
          <InfoIcon description="Guidance on scheduled statuses" />
        </ButtonText>
      }
    >
      <p>
        This is a summary of the different status types associated to approved
        scheduled releases.
      </p>
      <SummaryList>
        <SummaryListItem term={<Tag colour="orange">Validating</Tag>}>
          This is a release that has just been approved. The system is
          validating that everything is OK for publication
        </SummaryListItem>
        <SummaryListItem term={<Tag colour="blue">Scheduled</Tag>}>
          This is a release that has been approved and validated and is now
          scheduled for release on the 'Scheduled publish date'
        </SummaryListItem>
        <SummaryListItem term={<Tag colour="orange">Started</Tag>}>
          The publication process has now started
        </SummaryListItem>
        <SummaryListItem term={<Tag colour="green">Complete ✓</Tag>}>
          The release has now been successfully published and is now live for
          public view
        </SummaryListItem>
        <SummaryListItem term={<Tag colour="red">Failed ✖</Tag>}>
          There is a problem preventing the release from being successfully
          published. Contact{' '}
          <a href="mailto:explore.statistics@education.gov.uk">
            explore.statistics@education.gov.uk
          </a>{' '}
          for further assistance
        </SummaryListItem>
      </SummaryList>
      <p>
        For <Tag colour="red">Failed</Tag>, <Tag colour="orange">Started</Tag>{' '}
        or <Tag colour="green">Complete</Tag> releases clicking 'View stages'
        will show a summary of the publication status, highlighting all the
        publication steps and whether they have succeeded or failed
      </p>
    </Modal>
  );
}
