import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { Theme } from '@common/services/publicationService';
import { ThemeSummary } from '@common/services/themeService';
import React from 'react';

export default function ThemesModal({
  themes,
}: {
  themes: ThemeSummary[] | Theme[];
}) {
  return (
    <Modal
      className="govuk-!-width-one-half"
      description="This is a description list of our different publication themes."
      showClose
      title="Themes guidance"
      triggerButton={<ButtonText>What are themes?</ButtonText>}
    >
      <SummaryList>
        {themes.map(theme => (
          <SummaryListItem key={theme.id} term={theme.title}>
            {theme.summary}
          </SummaryListItem>
        ))}
      </SummaryList>
    </Modal>
  );
}
