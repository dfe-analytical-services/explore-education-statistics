import Button from '@common/components/Button';
import Modal from '@common/components/Modal';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import React from 'react';

export const MethodologyStatusGuidanceModal = ({
  open,
  onClose,
}: {
  open: boolean;
  onClose: () => void;
}) => (
  <Modal
    className="govuk-!-width-one-half"
    open={open}
    title="Methodology type guidance"
    onExit={onClose}
  >
    <p>A methodology can have various types of status as descibed below:</p>
    <SummaryList>
      <SummaryListItem term={<Tag>Draft</Tag>}>
        This is an unpublished draft methodology that can still be edited
      </SummaryListItem>
      <SummaryListItem term={<Tag>Approved</Tag>}>
        This is an approved methodology that can be published at the same time
        as a specific release
      </SummaryListItem>
      <SummaryListItem term={<Tag colour="green">Published</Tag>}>
        This is a published methodology that is live for public view
      </SummaryListItem>
    </SummaryList>
    <Button className="govuk-!-margin-bottom-0" onClick={onClose}>
      Close
    </Button>
  </Modal>
);

export const MethodologyTypeGuidanceModal = ({
  open,
  onClose,
}: {
  open: boolean;
  onClose: () => void;
}) => (
  <Modal
    className="govuk-!-width-one-half"
    open={open}
    title="Methodology type guidance"
    onExit={onClose}
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
    <Button className="govuk-!-margin-bottom-0" onClick={onClose}>
      Close
    </Button>
  </Modal>
);
