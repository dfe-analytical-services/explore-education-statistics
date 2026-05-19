import ButtonText from '@common/components/ButtonText';
import FormCheckbox from '@common/components/form/FormCheckbox';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import { PublicationTreeSummary } from '@common/services/publicationService';
import styles from '@frontend/modules/search-data/components/ThemesAndReleasesFilterGroup.module.scss';
import classNames from 'classnames';
import React from 'react';

interface ThemesAndReleasesFilterGroupReleasesProps {
  publicationIds: string[];
  publications: PublicationTreeSummary[];
  themeId: string;
  themeTitle: string;
  onChange: (themeId: string, pubId: string, isChecked: boolean) => void;
}

const ThemesAndReleasesFilterGroupReleases = ({
  publicationIds,
  publications,
  themeId,
  themeTitle,
  onChange,
}: ThemesAndReleasesFilterGroupReleasesProps) => {
  const [isExpanded, toggleExpanded] = useToggle(false);
  const publicationCountText = `${publications.length} statistical release${
    publications.length === 1 ? '' : 's'
  }`;

  if (!isExpanded) {
    return (
      <ButtonText
        className="govuk-!-font-size-16 govuk-!-display-block govuk-!-margin-bottom-2 govuk-!-margin-left-3"
        onClick={toggleExpanded}
      >
        Show {publicationCountText}
        <VisuallyHidden>{` for ${themeTitle}`}</VisuallyHidden>
      </ButtonText>
    );
  }

  return (
    <fieldset
      className={classNames(styles.expandedThemeContainer, 'govuk-fieldset')}
    >
      <legend className="govuk-fieldset__legend govuk-fieldset__legend--s govuk-!-margin-bottom-0">
        Statistical releases
      </legend>
      <ButtonText
        className="govuk-!-font-size-16 govuk-!-margin-bottom-2 govuk-!-display-block"
        onClick={toggleExpanded}
      >
        Close
        <VisuallyHidden>{` statistical releases for ${themeTitle}`}</VisuallyHidden>
      </ButtonText>

      <div className="govuk-checkboxes govuk-checkboxes--small">
        {publications.map(pub => (
          <FormCheckbox
            key={pub.id}
            id={`publication-${pub.id}`}
            name="publicationId"
            value={pub.id}
            label={pub.title}
            checked={publicationIds.includes(pub.id)}
            onChange={e => onChange(themeId, pub.id, e.target.checked)}
          />
        ))}
      </div>
    </fieldset>
  );
};

export default ThemesAndReleasesFilterGroupReleases;
