import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import Modal from '@common/components/Modal';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import React from 'react';

export function MethodologyStatusGuidanceModal() {
  return (
    <Modal
      className="govuk-!-width-one-half"
      showClose
      title="Methodology status guidance"
      triggerButton={
        <ButtonText>
          <InfoIcon description="Guidance on methodology statuses" />
        </ButtonText>
      }
    >
      <p>A methodology can have various types of status as described below:</p>
      <SummaryList>
        <SummaryListItem term={<Tag>Draft</Tag>}>
          This is an unpublished draft methodology that can still be edited
        </SummaryListItem>
        <SummaryListItem term={<Tag>In review</Tag>}>
          This is a methodology that is ready to be reviewed prior to
          publication
        </SummaryListItem>
        <SummaryListItem term={<Tag>Draft amendment</Tag>}>
          This is a published methodology that is currently being amended
        </SummaryListItem>
        <SummaryListItem term={<Tag>In review amendment</Tag>}>
          This is a published methodology that has an amendment that is ready to
          be reviewed prior to publication
        </SummaryListItem>
        <SummaryListItem term={<Tag>Approved</Tag>}>
          This is an approved methodology that can be published at the same time
          as a specific release
        </SummaryListItem>
        <SummaryListItem term={<Tag colour="green">Published</Tag>}>
          This is a published methodology that is live for public view
        </SummaryListItem>
      </SummaryList>
    </Modal>
  );
}

export function MethodologyTypeGuidanceModal() {
  return (
    <Modal
      className="govuk-!-width-one-half"
      showClose
      title="Methodology type guidance"
      triggerButton={
        <ButtonText>
          <InfoIcon description="Guidance on methodology types" />
        </ButtonText>
      }
    >
      <p>Various types of methodology can be associated to this publication:</p>
      <SummaryList>
        <SummaryListItem term="Owned">
          This is a methodology that belongs to and was created specifically for
          this publication
        </SummaryListItem>
        <SummaryListItem term="Adopted">
          This is a methodology that is owned by another publication, but can be
          selected to be shown with this publication
        </SummaryListItem>
        <SummaryListItem term="External">
          This is a link to an existing methodology that is hosted externally
        </SummaryListItem>
      </SummaryList>
    </Modal>
  );
}
