import Link from '@admin/components/Link';
import styles from '@admin/pages/release/content/components/ReleasePageTitle.module.scss';
import { ContentPublication } from '@admin/services/publicationService';
import { EditableRelease } from '@admin/services/releaseContentService';
import releaseFileService from '@admin/services/releaseFileService';
import ButtonText from '@common/components/ButtonText';
import { useMobileMedia } from '@common/hooks/useMedia';
import React from 'react';

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

      <div className="govuk-grid-column-one-third">
        <h2
          className="govuk-heading-m govuk-!-margin-bottom-2"
          id="quick-links"
          style={{ borderTop: '3px solid #1d70b8', paddingTop: '1rem' }}
        >
          Quick links
        </h2>
        <nav
          role="navigation"
          aria-labelledby="quick-links"
          data-testid="quick-links"
        >
          <ul className="govuk-list dfe-flex dfe-flex-direction--column dfe-gap-2">
            <li>
              <ButtonText
                preventDoubleClick
                onClick={() =>
                  releaseFileService.downloadFilesAsZip(release.id)
                }
              >
                Download all data (ZIP)
              </ButtonText>
            </li>
            <li>
              <span>Create your own tables</span>
            </li>
            <li>
              <Link to={`/subscriptions/new-subscription/${publication.slug}`}>
                Get email alerts
              </Link>
            </li>
          </ul>
        </nav>
      </div>
    </div>
  );
};
export default ReleasePageTitle;
