import styles from '@admin/pages/release/content/components/ReleasePageTitle.module.scss';
import { ContentPublication } from '@admin/services/publicationService';
import { EditableRelease } from '@admin/services/releaseContentService';
import { useMobileMedia } from '@common/hooks/useMedia';
import React from 'react';
import ReleasePageTitleQuickLinks from './ReleasePageQuickLinks';

interface Props {
  publication: ContentPublication;
  release: EditableRelease;
}

const ReleasePageTitle = ({ publication, release }: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  return (
    <div className={styles.releasePageTitleWrap}>
      <div className={styles.releasePageTitle}>
        <span className="govuk-caption-xl" data-testid="page-title-caption">
          {release.title}
        </span>
        <h2
          className="govuk-heading-xl govuk-!-margin-bottom-2"
          data-testid="page-title"
        >
          {publication.title}
        </h2>
        {!isMobileMedia && (
          <p className="govuk-body-l govuk-!-margin-bottom-0">
            {publication.summary}
          </p>
        )}
      </div>

      {!isMobileMedia && (
        <ReleasePageTitleQuickLinks
          publication={publication}
          release={release}
        />
      )}
    </div>
  );
};

export default ReleasePageTitle;
